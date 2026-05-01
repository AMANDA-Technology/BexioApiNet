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
using BexioApiNet.Abstractions.Models.Items.Items.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Items;

/// <summary>
/// Live end-to-end tests for the item connector exposed via
/// <see cref="IBexioApiClient.Items"/>. Tests are skipped when the required environment
/// variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are not present.
/// The CRUD lifecycle test creates and deletes its own item in <c>try</c>/<c>finally</c>
/// so the live tenant is left in a clean state.
/// </summary>
[Category("E2E")]
public sealed class ItemServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists the first page of items and asserts the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task Get_ReturnsItems()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Items.Get(new QueryParameterItem(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Fetches a single item by id using the first one returned from the list endpoint
    /// and asserts round-trip equality on the id.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsItem()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var list = await BexioApiClient!.Items.Get(new QueryParameterItem(Limit: 1));
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no items available on this tenant");
            return;
        }

        var result = await BexioApiClient.Items.GetById(existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data?.Id, Is.EqualTo(existing[0].Id));
        });
    }

    /// <summary>
    /// Searches items by intern_name and asserts the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task Search_ReturnsItems()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "intern_name", Value = "", Criteria = "like" }
        };

        var result = await BexioApiClient!.Items.Search(criteria);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies the full Create → Read → Update → Delete lifecycle for a service-type item.
    /// The item is deleted in <c>finally</c> so the tenant is left clean even when an
    /// intermediate assertion fails.
    /// </summary>
    [Test]
    public async Task Lifecycle_CreateReadUpdateDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var internCode = $"E2E-{Guid.NewGuid():N}";
        var create = new ItemCreate(
            UserId: 1,
            ArticleTypeId: 2,
            InternCode: internCode,
            InternName: $"E2E item {internCode}");

        var created = await BexioApiClient!.Items.Create(create);

        Assert.That(created, Is.Not.Null);
        Assert.That(created.IsSuccess, Is.True, () => $"create failed: {created.ApiError?.Message}");
        Assert.That(created.Data, Is.Not.Null);

        var id = created.Data!.Id;

        try
        {
            var fetched = await BexioApiClient.Items.GetById(id);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(id));
                Assert.That(fetched.Data!.InternCode, Is.EqualTo(internCode));
            });

            var updatedName = $"E2E item updated {internCode}";
            var update = new ItemUpdate(
                UserId: 1,
                InternCode: internCode,
                InternName: updatedName);

            var updated = await BexioApiClient.Items.Update(id, update);
            Assert.Multiple(() =>
            {
                Assert.That(updated.IsSuccess, Is.True);
                Assert.That(updated.Data, Is.Not.Null);
                Assert.That(updated.Data!.InternName, Is.EqualTo(updatedName));
            });
        }
        finally
        {
            await BexioApiClient.Items.Delete(id);
        }
    }
}
