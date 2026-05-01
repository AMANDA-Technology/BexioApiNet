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
using BexioApiNet.Abstractions.Models.MasterData.Titles.Views;

namespace BexioApiNet.E2eTests.Tests.MasterData.Titles;

/// <summary>
/// Live E2E tests for <see cref="BexioApiNet.Services.Connectors.MasterData.TitleService"/>.
/// Exercises the full Create → Read → Update → Search → Delete lifecycle of the v3.0.0
/// <c>/2.0/title</c> endpoints (see <see href="https://docs.bexio.com/#tag/Titles" />),
/// asserting the response payloads round-trip through the canonical Title schema.
/// All test data is prefixed with <c>E2E-</c> so it can be identified and cleaned up if a
/// run is interrupted before the tear-down branch executes.
/// </summary>
[Category("E2E")]
public sealed class TitleServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists titles and asserts the array deserializes — the test tenant should always
    /// contain at least the default Bexio entries.
    /// </summary>
    [Test]
    public async Task Get_ReturnsTitleList()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Titles.Get();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ApiError, Is.Null);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var first = result.Data!.First();
        Assert.Multiple(() =>
        {
            Assert.That(first.Id, Is.GreaterThan(0));
            Assert.That(first.Name, Is.Not.Null.And.Not.Empty);
        });
    }

    /// <summary>
    /// Walks the full lifecycle: Create a unique title, fetch it by id, search for it,
    /// update its name and finally delete it. Cleanup runs in <c>finally</c> so an orphan
    /// row is not left behind even when an assertion fails mid-run.
    /// </summary>
    [Test]
    public async Task CreateGetUpdateSearchAndDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var name = $"E2E-Title-{Random.Shared.Next(100000, 999999)}";

        var created = await BexioApiClient!.Titles.Create(new TitleCreate(name));

        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess, Is.True);
            Assert.That(created.ApiError, Is.Null);
            Assert.That(created.Data, Is.Not.Null);
            Assert.That(created.Data!.Id, Is.GreaterThan(0));
            Assert.That(created.Data.Name, Is.EqualTo(name));
        });

        try
        {
            var fetched = await BexioApiClient.Titles.GetById(created.Data!.Id);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(created.Data.Id));
                Assert.That(fetched.Data.Name, Is.EqualTo(name));
            });

            var searchResult = await BexioApiClient.Titles.Search(
            [
                new SearchCriteria { Field = "name", Value = name, Criteria = "=" }
            ]);

            Assert.Multiple(() =>
            {
                Assert.That(searchResult.IsSuccess, Is.True);
                Assert.That(searchResult.Data, Is.Not.Null);
                Assert.That(searchResult.Data!, Has.Some.Matches<BexioApiNet.Abstractions.Models.MasterData.Titles.Title>(
                    t => t.Id == created.Data.Id));
            });

            var newName = $"{name}-upd";
            var updated = await BexioApiClient.Titles.Update(created.Data.Id, new TitleUpdate(newName));
            Assert.Multiple(() =>
            {
                Assert.That(updated.IsSuccess, Is.True);
                Assert.That(updated.Data, Is.Not.Null);
                Assert.That(updated.Data!.Id, Is.EqualTo(created.Data.Id));
                Assert.That(updated.Data.Name, Is.EqualTo(newName));
            });
        }
        finally
        {
            var deleted = await BexioApiClient.Titles.Delete(created.Data!.Id);
            Assert.That(deleted.IsSuccess, Is.True);
        }
    }
}
