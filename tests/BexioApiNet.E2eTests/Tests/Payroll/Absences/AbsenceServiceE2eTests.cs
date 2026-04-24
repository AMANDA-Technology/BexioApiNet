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

namespace BexioApiNet.E2eTests.Tests.Payroll.Absences;

/// <summary>
/// Live E2E tests for the <see cref="BexioApiNet.Services.Connectors.Payroll.AbsenceService"/>.
/// Read-only calls only: listing absences for a placeholder employee. Tests are
/// auto-skipped when credentials are missing per <see cref="BexioE2eTestBase"/>.
/// </summary>
[Category("E2E")]
public sealed class AbsenceServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists payroll absences for a placeholder employee id and asserts the response
    /// deserializes correctly — the call must succeed and <c>Data</c> must be populated
    /// (possibly empty). The id is a dummy because this stub is auto-skipped without
    /// credentials.
    /// </summary>
    [Test]
    public async Task Get_ReturnsListResult()
    {
        var employeeId = Guid.Empty;
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.PayrollAbsences.Get(employeeId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
