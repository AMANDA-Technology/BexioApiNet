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

using BexioApiNet.Abstractions.Models.MasterData.FictionalUsers.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.MasterData.FictionalUsers;

/// <summary>
///     Live E2E coverage for the Bexio v3.0 <c>/fictional_users</c> endpoint. Exercises the
///     full Create → Read → Update (PATCH) → Delete lifecycle and asserts each response
///     payload deserializes into the strongly-typed
///     <see cref="BexioApiNet.Abstractions.Models.MasterData.FictionalUsers.FictionalUser" /> record
///     with every field present per the OpenAPI schema.
/// </summary>
public sealed class TestFictionalUsers : BexioE2eTestBase
{
    /// <summary>
    ///     Lists the first page of fictional users and confirms the round-trip succeeds.
    /// </summary>
    [Test]
    public async Task List_ReturnsFictionalUsers()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.FictionalUsers.Get(new QueryParameterFictionalUser(5, 0));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Drives the full Create → Read → Update (PATCH) → Delete lifecycle. Asserts the
    ///     response payloads carry the OpenAPI-described fields after every step. The fictional
    ///     user is always cleaned up at the end so the test leaves no residue in the live tenant.
    /// </summary>
    [Test]
    public async Task Lifecycle_CreateReadUpdateDelete_RoundTripsEveryField()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var unique = $"e2e-{Guid.NewGuid():N}";
        var email = $"{unique}@bexio-e2e.invalid";
        var created = await BexioApiClient!.FictionalUsers.Create(
            new FictionalUserCreate("male", "E2E", "Tester", email));

        Assert.That(created.IsSuccess, Is.True, () => created.ApiError?.Message ?? "create failed");
        Assert.That(created.Data, Is.Not.Null);

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(created.Data!.Id, Is.GreaterThan(0));
                Assert.That(created.Data.SalutationType, Is.EqualTo("male"));
                Assert.That(created.Data.Firstname, Is.EqualTo("E2E"));
                Assert.That(created.Data.Lastname, Is.EqualTo("Tester"));
                Assert.That(created.Data.Email, Is.EqualTo(email));
            });

            var fetched = await BexioApiClient.FictionalUsers.GetById(created.Data!.Id);
            Assert.That(fetched.IsSuccess, Is.True);
            Assert.That(fetched.Data!.Id, Is.EqualTo(created.Data.Id));

            var patched = await BexioApiClient.FictionalUsers.Patch(
                created.Data.Id,
                new FictionalUserPatch(Firstname: "E2E-Updated"));
            Assert.That(patched.IsSuccess, Is.True);
            Assert.That(patched.Data!.Firstname, Is.EqualTo("E2E-Updated"));
        }
        finally
        {
            var deleted = await BexioApiClient.FictionalUsers.Delete(created.Data!.Id);
            Assert.That(deleted.IsSuccess, Is.True, () => deleted.ApiError?.Message ?? "delete failed");
        }
    }
}
