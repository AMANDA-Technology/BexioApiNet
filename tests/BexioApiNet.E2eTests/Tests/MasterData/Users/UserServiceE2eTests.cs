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

namespace BexioApiNet.E2eTests.Tests.MasterData.Users;

/// <summary>
/// Live E2E tests for <see cref="BexioApiNet.Services.Connectors.MasterData.UserService"/>.
/// The Bexio v3.0 user-management endpoints are read-only — no <c>POST</c>/<c>PUT</c>/<c>DELETE</c>
/// operations are exposed (see <see href="https://docs.bexio.com/#tag/User-Management" />).
/// This fixture exercises the list, singleton (<c>/users/me</c>) and single-item paths
/// against the live tenant and asserts the User payload deserializes.
/// </summary>
[Category("E2E")]
public sealed class UserServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists users and asserts the array deserializes — the test tenant should always
    /// contain at least the authenticated user.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsUserList()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Users.GetAll();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ApiError, Is.Null);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var first = result.Data!.First();
        Assert.Multiple(() =>
        {
            Assert.That(first.Id, Is.GreaterThan(0));
            Assert.That(first.Email, Is.Not.Null.And.Not.Empty);
        });
    }

    /// <summary>
    /// Fetches the authenticated user via <c>/3.0/users/me</c> and asserts the payload
    /// deserializes with a populated email field.
    /// </summary>
    [Test]
    public async Task GetMe_ReturnsAuthenticatedUser()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Users.GetMe();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.GreaterThan(0));
            Assert.That(result.Data.Email, Is.Not.Null.And.Not.Empty);
        });
    }

    /// <summary>
    /// Fetches the first user from the list endpoint and validates that
    /// <see cref="BexioApiNet.Services.Connectors.MasterData.UserService.GetById" /> returns
    /// the same record on round-trip.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsUser()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var listResult = await BexioApiClient!.Users.GetAll();

        Assert.That(listResult.IsSuccess, Is.True);
        Assert.That(listResult.Data, Is.Not.Null.And.Not.Empty);

        var firstId = listResult.Data!.First().Id;

        var result = await BexioApiClient.Users.GetById(firstId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(firstId));
            Assert.That(result.Data.Email, Is.Not.Null.And.Not.Empty);
        });
    }
}
