/*
MIT License

Copyright (c) 2022 Philip Näf <philip.naef@amanda-technology.ch>
Copyright (c) 2022 Manuel Gysin <manuel.gysin@amanda-technology.ch>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Payroll.Absences;
using BexioApiNet.Abstractions.Models.Payroll.Absences.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Payroll;

namespace BexioApiNet.UnitTests.Payroll;

/// <summary>
/// Offline unit tests for <see cref="AbsenceService"/>. Each test asserts that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected verb, path,
/// query string and payload, and returns the handler's <see cref="ApiResult{T}"/> unchanged.
/// Verifies that absences are routed under the nested
/// <c>4.0/payroll/employees/{employeeId}/absences</c> path, that <c>Get</c> sends the
/// required <c>businessYear</c> query parameter and that <c>Update</c> uses <c>PUT</c> per
/// the Bexio v4.0 convention. No network, no filesystem.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class AbsenceServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "4.0/payroll/employees";

    private static readonly Guid EmployeeId = Guid.NewGuid();

    private AbsenceService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="AbsenceService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new AbsenceService(ConnectionHandler);
    }

    /// <summary>
    /// <c>Get</c> forwards the <c>businessYear</c> query parameter to
    /// <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> at the nested absences
    /// collection path under the supplied employee id.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithEmployeeIdInPathAndBusinessYearQueryParameter()
    {
        const int businessYear = 2026;
        var response = new ApiResult<AbsenceListResponse>
        {
            IsSuccess = true,
            Data = new AbsenceListResponse([])
        };
        QueryParameter? capturedQuery = null;
        ConnectionHandler
            .GetAsync<AbsenceListResponse>(
                Arg.Any<string>(),
                Arg.Do<QueryParameter?>(q => capturedQuery = q),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(EmployeeId, businessYear);

        await ConnectionHandler.Received(1).GetAsync<AbsenceListResponse>(
            $"{ExpectedEndpoint}/{EmployeeId}/absences",
            Arg.Any<QueryParameter?>(),
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
        Assert.That(capturedQuery, Is.Not.Null);
        Assert.That(capturedQuery!.Parameters["businessYear"], Is.EqualTo(businessYear));
    }

    /// <summary>
    /// <c>GetById</c> calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with both
    /// the employee id and the absence id appended to the endpoint path and a
    /// <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithEmployeeAndAbsenceIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Absence>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Absence> { IsSuccess = true });

        await _sut.GetById(EmployeeId, id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{EmployeeId}/absences/{id}"));
        await ConnectionHandler.Received(1).GetAsync<Absence>(
            $"{ExpectedEndpoint}/{EmployeeId}/absences/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Create</c> forwards the <see cref="AbsenceCreate"/> payload and the nested endpoint
    /// path to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithEmployeeIdInPath()
    {
        var payload = new AbsenceCreate(
            Reason: "Sickness",
            StartDate: new DateOnly(2026, 1, 10),
            EndDate: new DateOnly(2026, 1, 12));
        var response = new ApiResult<Absence> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Absence, AbsenceCreate>(Arg.Any<AbsenceCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(EmployeeId, payload);

        await ConnectionHandler.Received(1).PostAsync<Absence, AbsenceCreate>(
            payload,
            $"{ExpectedEndpoint}/{EmployeeId}/absences",
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// <c>Update</c> calls <see cref="IBexioConnectionHandler.PutAsync{TResult,TUpdate}"/>
    /// (PUT, not PATCH) with both the employee id and the absence id appended to
    /// the nested endpoint path.
    /// </summary>
    [Test]
    public async Task Update_CallsPutAsync_WithEmployeeAndAbsenceIdInPath()
    {
        var id = Guid.NewGuid();
        var payload = new AbsenceUpdate(
            Reason: "Vacation",
            StartDate: new DateOnly(2026, 6, 1),
            EndDate: new DateOnly(2026, 6, 14),
            HalfDay: false,
            ContinuedPay: 100m,
            Disability: 0m,
            PaidHours: 80m);
        string? capturedPath = null;
        ConnectionHandler
            .PutAsync<Absence, AbsenceUpdate>(
                Arg.Any<AbsenceUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Absence> { IsSuccess = true });

        await _sut.Update(EmployeeId, id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{EmployeeId}/absences/{id}"));
        await ConnectionHandler.Received(1).PutAsync<Absence, AbsenceUpdate>(
            payload,
            $"{ExpectedEndpoint}/{EmployeeId}/absences/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Delete</c> calls <see cref="IBexioConnectionHandler.Delete"/> with both the
    /// employee id and the absence id appended to the nested endpoint path.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete_WithEmployeeAndAbsenceIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .Delete(Arg.Do<string>(path => capturedPath = path), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<object> { IsSuccess = true });

        await _sut.Delete(EmployeeId, id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{EmployeeId}/absences/{id}"));
        await ConnectionHandler.Received(1).Delete(
            $"{ExpectedEndpoint}/{EmployeeId}/absences/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Delete</c> returns the <see cref="ApiResult{T}"/> from the connection handler
    /// unchanged.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(EmployeeId, Guid.NewGuid());

        Assert.That(result, Is.SameAs(response));
    }
}
