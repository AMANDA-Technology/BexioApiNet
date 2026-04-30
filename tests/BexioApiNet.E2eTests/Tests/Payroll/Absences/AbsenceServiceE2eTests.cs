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

namespace BexioApiNet.E2eTests.Tests.Payroll.Absences;

/// <summary>
/// Live E2E tests for the <see cref="BexioApiNet.Services.Connectors.Payroll.AbsenceService"/>.
/// Exercises the full Bexio v4.0
/// <c>/4.0/payroll/employees/{employeeId}/absences</c> surface, including the
/// Create → Read → Update → Delete lifecycle when a real employee is available on the
/// account. Tests are auto-skipped when credentials are missing per
/// <see cref="BexioE2eTestBase"/>.
/// </summary>
[Category("E2E")]
public sealed class AbsenceServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists payroll absences for the first available employee in the current business
    /// year and asserts that the response deserializes into the v4.0 envelope shape
    /// (<c>{ data: [...] }</c>) where each entry exposes <c>reason</c>,
    /// <c>start_date</c> and <c>id</c> per the OpenAPI spec.
    /// </summary>
    [Test]
    public async Task Get_ReturnsListResult()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var employees = await BexioApiClient!.PayrollEmployees.Get();
        Assert.That(employees.IsSuccess, Is.True);
        Assert.That(employees.Data, Is.Not.Null);

        if (employees.Data!.Data.Count is 0)
            Assert.Ignore("No payroll employees available on the test account.");

        var employeeId = employees.Data.Data[0].Id;
        var businessYear = DateTime.UtcNow.Year;

        var result = await BexioApiClient!.PayrollAbsences.Get(employeeId, businessYear);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Data, Is.Not.Null);
        });

        foreach (var absence in result.Data!.Data)
        {
            Assert.Multiple(() =>
            {
                Assert.That(absence.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(absence.Reason, Is.Not.Null.And.Not.Empty);
                Assert.That(absence.StartDate, Is.Not.EqualTo(default(DateOnly)));
            });
        }
    }

    /// <summary>
    /// Walks the full Create → Read → Update → Delete lifecycle for an absence on the
    /// first available payroll employee. The created absence is always cleaned up,
    /// even if intermediate assertions fail, so the test remains idempotent.
    /// </summary>
    [Test]
    public async Task CreateReadUpdateAndDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var employees = await BexioApiClient!.PayrollEmployees.Get();
        Assert.That(employees.IsSuccess, Is.True);
        Assert.That(employees.Data, Is.Not.Null);

        if (employees.Data!.Data.Count is 0)
            Assert.Ignore("No payroll employees available on the test account.");

        var employeeId = employees.Data.Data[0].Id;
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var endDate = startDate.AddDays(2);

        var created = await BexioApiClient!.PayrollAbsences.Create(employeeId, new AbsenceCreate(
            Reason: "Sickness",
            StartDate: startDate,
            EndDate: endDate,
            HalfDay: false,
            ContinuedPay: 100m,
            Disability: 0m,
            PaidHours: 16m));

        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess, Is.True);
            Assert.That(created.ApiError, Is.Null);
            Assert.That(created.Data, Is.Not.Null);
            Assert.That(created.Data!.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(created.Data!.Reason, Is.EqualTo("Sickness"));
        });

        try
        {
            var fetched = await BexioApiClient!.PayrollAbsences.GetById(employeeId, created.Data!.Id);

            Assert.That(fetched, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(created.Data!.Id));
            });

            var updated = await BexioApiClient!.PayrollAbsences.Update(employeeId, created.Data!.Id, new AbsenceUpdate(
                Reason: "Vacation",
                StartDate: startDate,
                EndDate: endDate,
                HalfDay: false,
                ContinuedPay: 100m,
                Disability: 0m,
                PaidHours: 16m));

            Assert.That(updated, Is.Not.Null);
            Assert.That(updated.IsSuccess, Is.True);
        }
        finally
        {
            var deleted = await BexioApiClient!.PayrollAbsences.Delete(employeeId, created.Data!.Id);
            Assert.That(deleted, Is.Not.Null);
            Assert.That(deleted.IsSuccess, Is.True);
        }
    }
}
