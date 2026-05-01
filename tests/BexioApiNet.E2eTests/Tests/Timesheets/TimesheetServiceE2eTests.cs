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
using BexioApiNet.Abstractions.Models.Timesheets.Timesheet;
using BexioApiNet.Abstractions.Models.Timesheets.Timesheet.Views;

namespace BexioApiNet.E2eTests.Tests.Timesheets;

/// <summary>
/// Live E2E coverage for the Bexio v2 timesheets endpoints (<c>GET/POST /2.0/timesheet</c>,
/// <c>GET/POST/DELETE /2.0/timesheet/{id}</c>, <c>POST /2.0/timesheet/search</c>). Skipped
/// automatically when credentials are absent. Bexio v2 timesheet edits use
/// <c>POST /2.0/timesheet/{id}</c> rather than PUT.
/// </summary>
public sealed class TimesheetServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Retrieves all timesheet entries from the live Bexio tenant and asserts the
    /// response is successful and the list shape is correct.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsListResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.Timesheets.Get();

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Posts a search criteria list against <c>/2.0/timesheet/search</c> and verifies the call
    /// succeeds. Uses a high-cardinality field (<c>id</c>) with a non-restrictive criteria
    /// so the request always validates against Bexio's spec regardless of tenant data.
    /// </summary>
    [Test]
    public async Task Search_ReturnsListResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "id", Value = "0", Criteria = ">" }
        };

        var res = await BexioApiClient!.Timesheets.Search(criteria);

        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Fetches the first timesheet returned by the list endpoint (when any exist) and
    /// asserts the full timesheet payload deserializes correctly through
    /// <see cref="BexioApiNet.Services.Connectors.Timesheets.TimesheetService.GetById"/>.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsTimesheetOrIgnoresWhenNoneExist()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var listResult = await BexioApiClient!.Timesheets.Get();
        Assert.That(listResult.IsSuccess, Is.True);

        if (listResult.Data is not { Count: > 0 })
        {
            Assert.Ignore("no timesheets available in the target Bexio account");
            return;
        }

        var firstId = listResult.Data[0].Id;
        var res = await BexioApiClient.Timesheets.GetById(firstId);

        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Id, Is.EqualTo(firstId));
            Assert.That(res.Data.Tracking, Is.Not.Null);
        });
    }

    /// <summary>
    /// Exercises the full Create → Read → Update → Delete lifecycle against the live API.
    /// The test discovers compatible <c>user_id</c> / <c>client_service_id</c> /
    /// <c>status_id</c> values from existing timesheets so the request is accepted by
    /// the tenant. Skipped when no existing timesheets are available to source those ids.
    /// </summary>
    [Test]
    public async Task Lifecycle_CreateGetUpdateDelete_RoundTrips()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var existing = await BexioApiClient!.Timesheets.Get();
        Assert.That(existing.IsSuccess, Is.True);

        if (existing.Data is not { Count: > 0 })
        {
            Assert.Ignore("no existing timesheets available to source compatible user_id / client_service_id / status_id");
            return;
        }

        var seed = existing.Data[0];

        var createPayload = new TimesheetCreate(
            UserId: seed.UserId,
            ClientServiceId: seed.ClientServiceId,
            AllowableBill: true,
            Tracking: new TimesheetDurationTracking(DateOnly.FromDateTime(DateTime.UtcNow.Date), "00:15"),
            StatusId: seed.StatusId,
            Text: "BexioApiNet E2E lifecycle probe");

        var createResult = await BexioApiClient.Timesheets.Create(createPayload);

        Assert.That(createResult.IsSuccess, Is.True, createResult.ApiError?.Message);
        Assert.That(createResult.Data, Is.Not.Null);
        var createdId = createResult.Data!.Id;

        try
        {
            var getResult = await BexioApiClient.Timesheets.GetById(createdId);
            Assert.That(getResult.IsSuccess, Is.True);
            Assert.That(getResult.Data!.Id, Is.EqualTo(createdId));

            var updatePayload = new TimesheetUpdate(
                UserId: seed.UserId,
                ClientServiceId: seed.ClientServiceId,
                AllowableBill: false,
                Tracking: new TimesheetDurationTracking(DateOnly.FromDateTime(DateTime.UtcNow.Date), "00:30"),
                StatusId: seed.StatusId,
                Text: "BexioApiNet E2E lifecycle probe (updated)");

            var updateResult = await BexioApiClient.Timesheets.Update(createdId, updatePayload);
            Assert.That(updateResult.IsSuccess, Is.True, updateResult.ApiError?.Message);
            Assert.That(updateResult.Data!.Id, Is.EqualTo(createdId));
        }
        finally
        {
            var deleteResult = await BexioApiClient.Timesheets.Delete(createdId);
            Assert.That(deleteResult.IsSuccess, Is.True, deleteResult.ApiError?.Message);
        }
    }
}
