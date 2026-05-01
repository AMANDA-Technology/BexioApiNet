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
using BexioApiNet.Abstractions.Models.BusinessActivities.BusinessActivity.Views;

namespace BexioApiNet.E2eTests.Tests.BusinessActivities;

/// <summary>
/// Live end-to-end tests for <c>BexioApiClient.BusinessActivities</c> against the Bexio
/// v3 OpenAPI spec (<c>/2.0/client_service</c>). Skipped when the
/// <c>BexioApiNet__BaseUri</c> / <c>BexioApiNet__JwtToken</c> environment variables are
/// not configured (see <see cref="BexioE2eTestBase"/>). The Bexio API exposes only GET,
/// POST and POST /search for business activities — there is no DELETE endpoint, so the
/// fixture verifies Create → Read → Search lifecycle but cannot tear created entries down.
/// </summary>
public sealed class BusinessActivityServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Fetches the list of business activities from the live Bexio API and verifies
    /// every record has the schema-required <c>id</c> and <c>name</c>.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsListWithRequiredFields()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.BusinessActivities.Get();

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });

        if (res.Data is { Count: > 0 })
        {
            foreach (var activity in res.Data)
            {
                Assert.That(activity.Id, Is.GreaterThan(0));
                Assert.That(activity.Name, Is.Not.Null.And.Not.Empty);
            }
        }
    }

    /// <summary>
    /// Posts a fully-populated search criteria list against <c>/2.0/client_service/search</c>
    /// and verifies the call succeeds. The Bexio API documents <c>name</c> as the
    /// supported search field — pass a wildcard-like substring so the request never fails
    /// validation regardless of the target tenant's data.
    /// </summary>
    [Test]
    public async Task Search_ReturnsListResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "a", Criteria = "like" }
        };

        var res = await BexioApiClient!.BusinessActivities.Search(criteria);

        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Exercises Create → verify-via-Get round-trip on the live API. The Bexio business
    /// activity endpoints offer no DELETE, so the test does not attempt cleanup —
    /// activities created here will accumulate in the target tenant. Test names include
    /// a deterministic suffix so re-runs do not multiply unique entries silently.
    /// </summary>
    [Test]
    public async Task Create_PersistsActivityRetrievableViaGet()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var name = $"BexioApiNet E2E {Guid.NewGuid():N}";

        var createResult = await BexioApiClient!.BusinessActivities.Create(
            new BusinessActivityCreate(
                Name: name,
                DefaultIsBillable: true,
                DefaultPricePerHour: 100m,
                AccountId: null));

        Assert.That(createResult.IsSuccess, Is.True, createResult.ApiError?.Message);
        Assert.That(createResult.Data, Is.Not.Null);
        Assert.That(createResult.Data!.Name, Is.EqualTo(name));
        Assert.That(createResult.Data.Id, Is.GreaterThan(0));

        var listResult = await BexioApiClient.BusinessActivities.Get();
        Assert.That(listResult.IsSuccess, Is.True);
        Assert.That(listResult.Data, Is.Not.Null);
        Assert.That(listResult.Data!.Any(a => a.Id == createResult.Data.Id && a.Name == name), Is.True);
    }
}
