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

using BexioApiNet.Abstractions.Models.Payroll.Employees.Views;
using BexioApiNet.Services.Connectors.Payroll;

namespace BexioApiNet.IntegrationTests.Payroll;

/// <summary>
/// Integration tests for <see cref="EmployeeService"/> against WireMock stubs. Verifies the
/// path composed from <see cref="EmployeeConfiguration"/> (<c>4.0/payroll/employees</c>) reaches
/// the handler correctly, and that the expected HTTP verbs are used for each operation —
/// including <c>PATCH</c> (not <c>PUT</c>) for partial updates, which differs from the
/// v4.0 Bill / Expense convention.
/// </summary>
[Category("Integration")]
public sealed class EmployeeServiceIntegrationTests : IntegrationTestBase
{
    private const string EmployeesPath = "/4.0/payroll/employees";

    private static readonly Guid TestEmployeeId = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479");

    private const string EmployeeResponse = """
                                            {
                                                "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
                                                "first_name": "John",
                                                "last_name": "Doe",
                                                "email": "john.doe@example.com",
                                                "employment_status": "ACTIVE",
                                                "created_at": "2026-01-01T00:00:00"
                                            }
                                            """;

    private const string EmployeeListBody = """
                                            [
                                                {
                                                    "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
                                                    "first_name": "John",
                                                    "last_name": "Doe",
                                                    "email": "john.doe@example.com",
                                                    "employment_status": "ACTIVE",
                                                    "created_at": "2026-01-01T00:00:00"
                                                }
                                            ]
                                            """;

    /// <summary>
    /// <c>EmployeeService.Get</c> issues a <c>GET</c> against <c>/4.0/payroll/employees</c>
    /// and deserializes the list response on success.
    /// </summary>
    [Test]
    public async Task EmployeeService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(EmployeesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(EmployeeListBody));

        var service = new EmployeeService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!, Has.Count.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(EmployeesPath));
        });
    }

    /// <summary>
    /// <c>EmployeeService.GetById</c> issues a <c>GET</c> request with the employee id
    /// in the URL path and surfaces the returned employee on success.
    /// </summary>
    [Test]
    public async Task EmployeeService_GetById_SendsGetRequestWithIdInPath()
    {
        var expectedPath = $"{EmployeesPath}/{TestEmployeeId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(EmployeeResponse));

        var service = new EmployeeService(ConnectionHandler);

        var result = await service.GetById(TestEmployeeId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(TestEmployeeId));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>EmployeeService.Create</c> sends a <c>POST</c> request whose body contains
    /// the serialized <see cref="EmployeeCreate"/> payload.
    /// </summary>
    [Test]
    public async Task EmployeeService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(EmployeesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(EmployeeResponse));

        var service = new EmployeeService(ConnectionHandler);

        var payload = new EmployeeCreate(
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            EmploymentStatus: "ACTIVE");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(EmployeesPath));
            Assert.That(request.Body, Does.Contain("\"first_name\":\"John\""));
            Assert.That(request.Body, Does.Contain("\"employment_status\":\"ACTIVE\""));
        });
    }

    /// <summary>
    /// <c>EmployeeService.Patch</c> sends a <c>PATCH</c> request against
    /// <c>/4.0/payroll/employees/{id}</c> — employees use <c>PATCH</c> for partial updates,
    /// not <c>PUT</c>.
    /// </summary>
    [Test]
    public async Task EmployeeService_Patch_SendsPatchRequestWithIdInPath()
    {
        var expectedPath = $"{EmployeesPath}/{TestEmployeeId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPatch())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(EmployeeResponse));

        var service = new EmployeeService(ConnectionHandler);

        var payload = new EmployeePatch(FirstName: "Jane");

        var result = await service.Patch(TestEmployeeId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"first_name\":\"Jane\""));
        });
    }
}
