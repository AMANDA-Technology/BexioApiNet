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
using BexioApiNet.Abstractions.Models.MasterData.Notes.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.MasterData.Notes;

/// <summary>
///     Live E2E coverage for the Bexio v2 <c>/note</c> endpoint. Exercises List + Search
///     and the full Create → Read → Update (POST) → Delete lifecycle. The Bexio
///     <c>v2EditNote</c> operation uses POST against <c>/2.0/note/{id}</c>; this test
///     proves the connector and the
///     <see cref="BexioApiNet.Abstractions.Json.BexioDateTimeJsonConverter" /> round-trip
///     the space-separated <c>event_start</c> format the API actually returns.
/// </summary>
public sealed class TestNotes : BexioE2eTestBase
{
    private const int OwnerUserId = 1;

    /// <summary>
    ///     Lists the first page of notes and confirms the round-trip succeeds.
    /// </summary>
    [Test]
    public async Task List_ReturnsNotes()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Notes.Get(new QueryParameterNote(5, 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Drives the full Create → Read → Update (POST) → Delete lifecycle. Asserts the
    ///     subject and info round-trip through the API and exercises the
    ///     <see cref="BexioApiNet.Abstractions.Json.BexioDateTimeJsonConverter" /> on both
    ///     write and read paths. The note is always cleaned up so the test leaves no
    ///     residue in the live tenant.
    /// </summary>
    [Test]
    public async Task Lifecycle_CreateReadUpdateDelete_RoundTripsEveryField()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var unique = $"e2e-note-{Guid.NewGuid():N}";
        var eventStart = DateTime.UtcNow.AddMinutes(-5).TrimToSeconds();

        var created = await BexioApiClient!.Notes.Create(new NoteCreate(
            UserId: OwnerUserId,
            EventStart: eventStart,
            Subject: unique,
            Info: "E2E created"));

        Assert.That(created.IsSuccess, Is.True, () => created.ApiError?.Message ?? "create failed");
        Assert.That(created.Data, Is.Not.Null);

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(created.Data!.Id, Is.GreaterThan(0));
                Assert.That(created.Data.UserId, Is.EqualTo(OwnerUserId));
                Assert.That(created.Data.Subject, Is.EqualTo(unique));
                Assert.That(created.Data.Info, Is.EqualTo("E2E created"));
                Assert.That(created.Data.EventStart, Is.EqualTo(eventStart));
            });

            var fetched = await BexioApiClient.Notes.GetById(created.Data!.Id);
            Assert.That(fetched.IsSuccess, Is.True);
            Assert.That(fetched.Data!.Id, Is.EqualTo(created.Data.Id));

            var updated = await BexioApiClient.Notes.Update(
                created.Data.Id,
                new NoteUpdate(
                    UserId: OwnerUserId,
                    EventStart: eventStart,
                    Subject: $"{unique}-edited",
                    Info: "E2E updated"));
            Assert.That(updated.IsSuccess, Is.True, () => updated.ApiError?.Message ?? "update failed");
            Assert.That(updated.Data!.Subject, Is.EqualTo($"{unique}-edited"));
            Assert.That(updated.Data.Info, Is.EqualTo("E2E updated"));
        }
        finally
        {
            var deleted = await BexioApiClient.Notes.Delete(created.Data!.Id);
            Assert.That(deleted.IsSuccess, Is.True, () => deleted.ApiError?.Message ?? "delete failed");
        }
    }

    /// <summary>
    ///     Searches notes by <c>user_id</c> (a search field documented in the Bexio API).
    ///     Verifies the search round-trips successfully.
    /// </summary>
    [Test]
    public async Task Search_ByUserId_ReturnsResults()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Notes.Search(
        [
            new SearchCriteria { Field = "user_id", Value = OwnerUserId.ToString(), Criteria = "=" }
        ]);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}

internal static class DateTimeExtensions
{
    public static DateTime TrimToSeconds(this DateTime value)
    {
        return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
    }
}
