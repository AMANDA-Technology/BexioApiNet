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
using BexioApiNet.Abstractions.Models.Payroll.Employees;
using BexioApiNet.Abstractions.Models.Payroll.Employees.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Payroll;

namespace BexioApiNet.UnitTests.Payroll;

/// <summary>
/// Offline unit tests for <see cref="EmployeeService"/>. Each test asserts that the service
/// forwards its calls to <see cref="IBexioConnectionHandler"/> with the expected verb, path
/// and payload, and returns the handler's <see cref="ApiResult{T}"/> unchanged. Verifies
/// that <see cref="EmployeeService.Patch"/> routes to <c>PATCH</c> (not <c>PUT</c>) as per
/// the Bexio v4.0 <c>payroll/employees</c> semantics. No network, no filesystem.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class EmployeeServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "4.0/payroll/employees";

    private EmployeeService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="EmployeeService"/> bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler"/> substitute before each test.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new EmployeeService(ConnectionHandler);
    }

    /// <summary>
    /// Get forwards a <see langword="null"/> <see cref="QueryParameter"/> to
    /// <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> at the employees collection path.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithoutQueryParameter()
    {
        var response = new ApiResult<List<Employee>>
        {
            IsSuccess = true,
            Data = []
        };
        ConnectionHandler
            .GetAsync<List<Employee>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Employee>>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}"/> with the
    /// employee id appended to the endpoint root and a <see langword="null"/> query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Employee>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Employee> { IsSuccess = true });

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).GetAsync<Employee>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create forwards the <see cref="EmployeeCreate"/> payload and the endpoint path
    /// to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}"/>.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = new EmployeeCreate(FirstName: "John", LastName: "Doe", Email: "john@example.com");
        var response = new ApiResult<Employee> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Employee, EmployeeCreate>(Arg.Any<EmployeeCreate>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Employee, EmployeeCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Patch calls <see cref="IBexioConnectionHandler.PatchAsync{TResult,TPatch}"/> (PATCH, not PUT)
    /// with the employee id appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Patch_CallsPatchAsync_WithIdInPath()
    {
        var id = Guid.NewGuid();
        var payload = new EmployeePatch(FirstName: "Jane");
        string? capturedPath = null;
        ConnectionHandler
            .PatchAsync<Employee, EmployeePatch>(
                Arg.Any<EmployeePatch>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<Employee> { IsSuccess = true });

        await _sut.Patch(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).PatchAsync<Employee, EmployeePatch>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }
}
