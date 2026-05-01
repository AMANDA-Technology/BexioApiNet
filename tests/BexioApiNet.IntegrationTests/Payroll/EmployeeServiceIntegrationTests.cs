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

using BexioApiNet.Abstractions.Models.Payroll.Employees;
using BexioApiNet.Abstractions.Models.Payroll.Employees.Views;
using BexioApiNet.Services.Connectors.Payroll;

namespace BexioApiNet.IntegrationTests.Payroll;

/// <summary>
/// Integration tests for <see cref="EmployeeService"/> against WireMock stubs. Verifies
/// the path composed from <see cref="EmployeeConfiguration"/> (<c>4.0/payroll/employees</c>)
/// reaches the handler correctly, that the spec-required <c>date</c> query parameter is
/// appended on GET-by-id and that the expected HTTP verbs are used for each operation —
/// including <c>PATCH</c> (not <c>PUT</c>) for partial updates. Each response stub is a
/// fully-populated JSON payload matching the OpenAPI schema, so deserialization is
/// asserted against every field including the nested <see cref="EmployeeAddress"/>.
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
                                                "date_of_birth": "1990-04-15",
                                                "ahv_number": "756.1234.5678.97",
                                                "gender": "male",
                                                "nationality": "CH",
                                                "stay_permit_category": "B",
                                                "language": "de",
                                                "marital_status": "married",
                                                "email": "john.doe@example.com",
                                                "phone_number": "+41 79 123 45 67",
                                                "hours_per_week": 40.0,
                                                "employment_level": 1.0,
                                                "annual_vacation_days_total": 25,
                                                "address": {
                                                    "complementary_line": "Hinterhof",
                                                    "street": "Bahnhofstrasse 12",
                                                    "street_name": "Bahnhofstrasse",
                                                    "house_number": "12",
                                                    "postbox": "PO 99",
                                                    "locality": "Mitte",
                                                    "zip_code": "8001",
                                                    "city": "Zurich",
                                                    "country": "CH",
                                                    "canton": "ZH",
                                                    "municipality_id": "261"
                                                },
                                                "personal_number": "EMP-001",
                                                "iban": "CH9300762011623852957",
                                                "annual_vacation_days_used": 5,
                                                "annual_vacation_days_left": 20,
                                                "effective_working_hours_per_week": 40
                                            }
                                            """;

    private const string EmployeeListBody = """
                                            {
                                                "data": [
                                                    {
                                                        "id": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
                                                        "first_name": "John",
                                                        "last_name": "Doe",
                                                        "date_of_birth": "1990-04-15",
                                                        "ahv_number": "756.1234.5678.97",
                                                        "gender": "male",
                                                        "nationality": "CH",
                                                        "stay_permit_category": "B",
                                                        "language": "de",
                                                        "marital_status": "married",
                                                        "email": "john.doe@example.com",
                                                        "phone_number": "+41 79 123 45 67",
                                                        "hours_per_week": 40.0,
                                                        "employment_level": 1.0,
                                                        "annual_vacation_days_total": 25,
                                                        "address": {
                                                            "complementary_line": "Hinterhof",
                                                            "street": "Bahnhofstrasse 12",
                                                            "street_name": "Bahnhofstrasse",
                                                            "house_number": "12",
                                                            "postbox": "PO 99",
                                                            "locality": "Mitte",
                                                            "zip_code": "8001",
                                                            "city": "Zurich",
                                                            "country": "CH",
                                                            "canton": "ZH",
                                                            "municipality_id": "261"
                                                        },
                                                        "personal_number": "EMP-001",
                                                        "iban": "CH9300762011623852957"
                                                    }
                                                ]
                                            }
                                            """;

    /// <summary>
    /// <c>EmployeeService.Get</c> issues a <c>GET</c> against <c>/4.0/payroll/employees</c>
    /// and deserializes the <c>{ data: [...] }</c> envelope, exposing every base employee
    /// field — including the nested <see cref="EmployeeAddress"/> — on the C# model.
    /// </summary>
    [Test]
    public async Task EmployeeService_Get_SendsGetRequest_AndDeserializesAllFields()
    {
        Server
            .Given(Request.Create().WithPath(EmployeesPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(EmployeeListBody));

        var service = new EmployeeService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;
        var first = result.Data?.Data[0];

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Data, Has.Count.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(EmployeesPath));

            Assert.That(first, Is.Not.Null);
            Assert.That(first!.Id, Is.EqualTo(TestEmployeeId));
            Assert.That(first.FirstName, Is.EqualTo("John"));
            Assert.That(first.LastName, Is.EqualTo("Doe"));
            Assert.That(first.DateOfBirth, Is.EqualTo(new DateOnly(1990, 4, 15)));
            Assert.That(first.AhvNumber, Is.EqualTo("756.1234.5678.97"));
            Assert.That(first.Gender, Is.EqualTo("male"));
            Assert.That(first.Nationality, Is.EqualTo("CH"));
            Assert.That(first.StayPermitCategory, Is.EqualTo("B"));
            Assert.That(first.Language, Is.EqualTo("de"));
            Assert.That(first.MaritalStatus, Is.EqualTo("married"));
            Assert.That(first.Email, Is.EqualTo("john.doe@example.com"));
            Assert.That(first.PhoneNumber, Is.EqualTo("+41 79 123 45 67"));
            Assert.That(first.HoursPerWeek, Is.EqualTo(40.0m));
            Assert.That(first.EmploymentLevel, Is.EqualTo(1.0m));
            Assert.That(first.AnnualVacationDaysTotal, Is.EqualTo(25));
            Assert.That(first.PersonalNumber, Is.EqualTo("EMP-001"));
            Assert.That(first.Iban, Is.EqualTo("CH9300762011623852957"));

            Assert.That(first.Address, Is.Not.Null);
            Assert.That(first.Address!.ComplementaryLine, Is.EqualTo("Hinterhof"));
            Assert.That(first.Address.Street, Is.EqualTo("Bahnhofstrasse 12"));
            Assert.That(first.Address.StreetName, Is.EqualTo("Bahnhofstrasse"));
            Assert.That(first.Address.HouseNumber, Is.EqualTo("12"));
            Assert.That(first.Address.Postbox, Is.EqualTo("PO 99"));
            Assert.That(first.Address.Locality, Is.EqualTo("Mitte"));
            Assert.That(first.Address.ZipCode, Is.EqualTo("8001"));
            Assert.That(first.Address.City, Is.EqualTo("Zurich"));
            Assert.That(first.Address.Country, Is.EqualTo("CH"));
            Assert.That(first.Address.Canton, Is.EqualTo("ZH"));
            Assert.That(first.Address.MunicipalityId, Is.EqualTo("261"));
        });
    }

    /// <summary>
    /// <c>EmployeeService.GetById</c> issues a <c>GET</c> request with the employee id
    /// in the URL path and the spec-required <c>date</c> query parameter (formatted
    /// <c>yyyy-MM-dd</c>), and deserializes the extended computed fields
    /// (<c>annual_vacation_days_used</c>, <c>annual_vacation_days_left</c>,
    /// <c>effective_working_hours_per_week</c>) returned by the GET-by-id endpoint.
    /// </summary>
    [Test]
    public async Task EmployeeService_GetById_SendsGetRequestWithIdInPath_AndDateQuery()
    {
        var expectedPath = $"{EmployeesPath}/{TestEmployeeId}";
        var date = new DateOnly(2026, 1, 31);

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(EmployeeResponse));

        var service = new EmployeeService(ConnectionHandler);

        var result = await service.GetById(TestEmployeeId, date, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(TestEmployeeId));
            Assert.That(result.Data.AnnualVacationDaysUsed, Is.EqualTo(5));
            Assert.That(result.Data.AnnualVacationDaysLeft, Is.EqualTo(20));
            Assert.That(result.Data.EffectiveWorkingHoursPerWeek, Is.EqualTo(40));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Query, Does.ContainKey("date"));
            Assert.That(request.Query!["date"].ToString(), Is.EqualTo("2026-01-31"));
        });
    }

    /// <summary>
    /// <c>EmployeeService.Create</c> sends a <c>POST</c> request whose body contains the
    /// serialized <see cref="EmployeeCreate"/> payload. The spec marks <c>ahvNumber</c>
    /// (wire name <c>ahv_number</c>) required.
    /// </summary>
    [Test]
    public async Task EmployeeService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(EmployeesPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(EmployeeResponse));

        var service = new EmployeeService(ConnectionHandler);

        var payload = new EmployeeCreate(
            AhvNumber: "756.1234.5678.97",
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            Nationality: "CH",
            Language: "de",
            MaritalStatus: "married");

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(EmployeesPath));
            Assert.That(request.Body, Does.Contain("\"ahv_number\":\"756.1234.5678.97\""));
            Assert.That(request.Body, Does.Contain("\"first_name\":\"John\""));
            Assert.That(request.Body, Does.Contain("\"last_name\":\"Doe\""));
            Assert.That(request.Body, Does.Contain("\"email\":\"john.doe@example.com\""));
            Assert.That(request.Body, Does.Contain("\"nationality\":\"CH\""));
            Assert.That(request.Body, Does.Contain("\"language\":\"de\""));
            Assert.That(request.Body, Does.Contain("\"marital_status\":\"married\""));
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

        var payload = new EmployeePatch(AhvNumber: "756.1234.5678.97", FirstName: "Jane");

        var result = await service.Patch(TestEmployeeId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PATCH"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"first_name\":\"Jane\""));
            Assert.That(request.Body, Does.Contain("\"ahv_number\":\"756.1234.5678.97\""));
        });
    }
}
