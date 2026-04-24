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

namespace BexioApiNet.E2eTests.Tests.Timesheets;

/// <summary>
/// Live E2E coverage for the timesheet status lookup endpoint
/// (<c>GET /2.0/timesheet_status</c>). Skipped automatically when credentials are absent.
/// </summary>
public class TimesheetStatusServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Retrieves all timesheet statuses from the live Bexio tenant and asserts the
    /// response is successful with at least one status returned.
    /// </summary>
    [Test]
    public async Task GetAll()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.TimesheetStatuses.Get();

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null.And.Not.Empty);
            Assert.That(res.Data!.First().Name, Is.Not.Null);
        });
    }
}
