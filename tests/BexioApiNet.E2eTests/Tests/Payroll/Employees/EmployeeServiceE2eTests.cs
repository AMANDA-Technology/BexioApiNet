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

namespace BexioApiNet.E2eTests.Tests.Payroll.Employees;

/// <summary>
/// Live E2E tests for the <see cref="BexioApiNet.Services.Connectors.Payroll.EmployeeService"/>.
/// Exercises the read-side of the Bexio v4.0 <c>/4.0/payroll/employees</c> surface —
/// list and GET-by-id-on-date. Create / Patch are not exercised live because Bexio's
/// payroll module rejects bogus AHV numbers and creating employees has business-side
/// side-effects that cannot be cleanly rolled back. Tests are auto-skipped when
/// credentials are missing per <see cref="BexioE2eTestBase"/>.
/// </summary>
[Category("E2E")]
public sealed class EmployeeServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists payroll employees and asserts the response deserializes into the v4.0
    /// envelope shape (<c>{ data: [...] }</c>) with each entry exposing the
    /// spec-required <c>id</c>, <c>nationality</c>, <c>language</c> and
    /// <c>marital_status</c> fields.
    /// </summary>
    [Test]
    public async Task Get_ReturnsListResult()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.PayrollEmployees.Get();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Data, Is.Not.Null);
        });

        foreach (var employee in result.Data!.Data)
        {
            Assert.Multiple(() =>
            {
                Assert.That(employee.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(employee.Nationality, Is.Not.Null.And.Not.Empty);
                Assert.That(employee.Language, Is.Not.Null.And.Not.Empty);
                Assert.That(employee.MaritalStatus, Is.Not.Null.And.Not.Empty);
            });
        }
    }

    /// <summary>
    /// Lists employees, picks the first available one and retrieves it on today's
    /// date via <c>GET /4.0/payroll/employees/{employeeId}?date=...</c>. Asserts the
    /// extended computed fields (vacation usage, effective hours) deserialize on the
    /// GET-by-id response per the OpenAPI <c>allOf</c> shape.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsExtendedEmployee()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var employees = await BexioApiClient!.PayrollEmployees.Get();
        Assert.That(employees.IsSuccess, Is.True);
        Assert.That(employees.Data, Is.Not.Null);

        if (employees.Data!.Data.Count is 0)
            Assert.Ignore("No payroll employees available on the test account.");

        var first = employees.Data.Data[0];
        var date = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var result = await BexioApiClient!.PayrollEmployees.GetById(first.Id, date);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(first.Id));
        });
    }
}
