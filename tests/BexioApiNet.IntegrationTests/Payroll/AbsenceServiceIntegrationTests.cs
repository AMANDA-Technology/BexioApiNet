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

using BexioApiNet.Abstractions.Models.Payroll.Absences.Views;
using BexioApiNet.Services.Connectors.Payroll;

namespace BexioApiNet.IntegrationTests.Payroll;

/// <summary>
/// Integration tests for <see cref="AbsenceService"/> against WireMock stubs. Verifies the
/// nested path composed from <see cref="AbsenceConfiguration"/>
/// (<c>4.0/payroll/employees/{employeeId}/absences</c>) reaches the handler correctly,
/// and that the expected HTTP verbs are used for each operation — including <c>PUT</c>
/// for Update per the v4.0 convention.
/// </summary>
[Category("Integration")]
public sealed class AbsenceServiceIntegrationTests : IntegrationTestBase
{
    private static readonly Guid TestEmployeeId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");
    private static readonly Guid TestAbsenceId = Guid.Parse("a1b2c3d4-58cc-4372-a567-0e02b2c3d479");

    private static readonly string AbsencesPath = $"/4.0/payroll/employees/{TestEmployeeId}/absences";
    private static readonly string AbsencePathWithId = $"{AbsencesPath}/{TestAbsenceId}";

    private const string AbsenceResponse = """
                                           {
                                               "id": "a1b2c3d4-58cc-4372-a567-0e02b2c3d479",
                                               "employee_id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
                                               "absence_type": "SICK",
                                               "start_date": "2026-01-10T00:00:00",
                                               "end_date": "2026-01-12T00:00:00",
                                               "status": "APPROVED",
                                               "created_at": "2026-01-01T00:00:00"
                                           }
                                           """;

    private const string AbsenceListBody = """
                                           [
                                               {
                                                   "id": "a1b2c3d4-58cc-4372-a567-0e02b2c3d479",
                                                   "employee_id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
                                                   "absence_type": "SICK",
                                                   "start_date": "2026-01-10T00:00:00",
                                                   "end_date": "2026-01-12T00:00:00",
                                                   "status": "APPROVED",
                                                   "created_at": "2026-01-01T00:00:00"
                                               }
                                           ]
                                           """;

    /// <summary>
    /// <c>AbsenceService.Get</c> issues a <c>GET</c> against
    /// <c>/4.0/payroll/employees/{employeeId}/absences</c> and deserializes the list
    /// response on success.
    /// </summary>
    [Test]
    public async Task AbsenceService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(AbsencesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(AbsenceListBody));

        var service = new AbsenceService(ConnectionHandler);

        var result = await service.Get(TestEmployeeId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Has.Count.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AbsencesPath));
        });
    }

    /// <summary>
    /// <c>AbsenceService.GetById</c> issues a <c>GET</c> request with both the employee
    /// id and the absence id in the URL path and surfaces the returned absence on success.
    /// </summary>
    [Test]
    public async Task AbsenceService_GetById_SendsGetRequestWithIdsInPath()
    {
        Server
            .Given(Request.Create().WithPath(AbsencePathWithId).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(AbsenceResponse));

        var service = new AbsenceService(ConnectionHandler);

        var result = await service.GetById(TestEmployeeId, TestAbsenceId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(TestAbsenceId));
            Assert.That(result.Data.EmployeeId, Is.EqualTo(TestEmployeeId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AbsencePathWithId));
        });
    }

    /// <summary>
    /// <c>AbsenceService.Create</c> sends a <c>POST</c> request whose body contains
    /// the serialized <see cref="AbsenceCreate"/> payload.
    /// </summary>
    [Test]
    public async Task AbsenceService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(AbsencesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(AbsenceResponse));

        var service = new AbsenceService(ConnectionHandler);

        var payload = new AbsenceCreate(
            AbsenceType: "SICK",
            StartDate: new DateTime(2026, 1, 10),
            EndDate: new DateTime(2026, 1, 12),
            Status: "APPROVED");

        var result = await service.Create(TestEmployeeId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AbsencesPath));
            Assert.That(request.Body, Does.Contain("\"absence_type\":\"SICK\""));
            Assert.That(request.Body, Does.Contain("\"status\":\"APPROVED\""));
        });
    }

    /// <summary>
    /// <c>AbsenceService.Update</c> sends a <c>PUT</c> request against
    /// <c>/4.0/payroll/employees/{employeeId}/absences/{id}</c> — v4.0 uses <c>PUT</c>
    /// for updates.
    /// </summary>
    [Test]
    public async Task AbsenceService_Update_SendsPutRequestWithIdsInPath()
    {
        Server
            .Given(Request.Create().WithPath(AbsencePathWithId).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(AbsenceResponse));

        var service = new AbsenceService(ConnectionHandler);

        var payload = new AbsenceUpdate(Status: "APPROVED");

        var result = await service.Update(TestEmployeeId, TestAbsenceId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AbsencePathWithId));
            Assert.That(request.Body, Does.Contain("\"status\":\"APPROVED\""));
        });
    }

    /// <summary>
    /// <c>AbsenceService.Delete</c> issues a <c>DELETE</c> request that includes both
    /// the employee id and the absence id in the URL path.
    /// </summary>
    [Test]
    public async Task AbsenceService_Delete_SendsDeleteRequestWithIdsInPath()
    {
        Server
            .Given(Request.Create().WithPath(AbsencePathWithId).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var service = new AbsenceService(ConnectionHandler);

        var result = await service.Delete(TestEmployeeId, TestAbsenceId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AbsencePathWithId));
        });
    }
}
