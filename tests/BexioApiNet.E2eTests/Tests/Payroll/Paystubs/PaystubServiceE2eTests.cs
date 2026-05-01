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

namespace BexioApiNet.E2eTests.Tests.Payroll.Paystubs;

/// <summary>
///     Live E2E tests for the <see cref="BexioApiNet.Services.Connectors.Payroll.PaystubService" />.
///     Read-only call: requesting the paystub-pdf endpoint which returns a JSON envelope
///     containing the download <c>location</c> URI for the requested employee + period.
///     Uses the first available payroll employee on the account. Tests are auto-skipped
///     when credentials are missing per <see cref="BexioE2eTestBase" /> or when no
///     employees exist on the test account.
/// </summary>
[Category("E2E")]
public sealed class PaystubServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Resolves an employee from the active list, requests the paystub PDF for the
    ///     prior month and asserts that either a successful response with a populated
    ///     <c>Location</c> is returned, or that the API surfaces a structured error
    ///     payload (no paystub generated for that month). Both outcomes are valid
    ///     against the v4.0 spec.
    /// </summary>
    [Test]
    public async Task GetPdf_ReturnsLocationOrApiError()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var employees = await BexioApiClient!.PayrollEmployees.Get();
        Assert.That(employees.IsSuccess, Is.True);
        Assert.That(employees.Data, Is.Not.Null);

        if (employees.Data!.Data.Count is 0)
            Assert.Ignore("No payroll employees available on the test account.");

        var employeeId = employees.Data.Data[0].Id;
        var previousMonth = DateTime.UtcNow.Date.AddMonths(-1);

        var result = await BexioApiClient!.PayrollPaystubs.GetPdf(employeeId, previousMonth.Year, previousMonth.Month);

        Assert.That(result, Is.Not.Null);
        if (result.IsSuccess)
        {
            Assert.Multiple(() =>
            {
                Assert.That(result.ApiError, Is.Null);
                Assert.That(result.Data, Is.Not.Null);
                Assert.That(result.Data!.Location, Is.Not.Null);
            });
        }
        else
        {
            Assert.That(result.ApiError, Is.Not.Null);
        }
    }
}
