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

using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.IntegrationTests.MasterData;

/// <summary>
///     Integration tests covering the read entry points of <see cref="UserService" /> against
///     WireMock stubs. The Bexio v3.0 user-management endpoints are read-only — this fixture
///     exercises the list, singleton (<c>/users/me</c>) and single-item paths and verifies
///     payloads round-trip through the canonical <see cref="BexioApiNet.Abstractions.Models.MasterData.Users.User" />
///     record. See <see href="https://docs.bexio.com/#tag/User-Management" />.
/// </summary>
public sealed class UserServiceIntegrationTests : IntegrationTestBase
{
    private const string UsersPath = "/3.0/users";

    private const string UserResponse = """
                                        {
                                            "id": 4,
                                            "salutation_type": "male",
                                            "firstname": "Rudolph",
                                            "lastname": "Smith",
                                            "email": "rudolph.smith@example.com",
                                            "is_superadmin": true,
                                            "is_accountant": false
                                        }
                                        """;

    private const string UserListResponse = """
                                            [
                                                {
                                                    "id": 4,
                                                    "salutation_type": "male",
                                                    "firstname": "Rudolph",
                                                    "lastname": "Smith",
                                                    "email": "rudolph.smith@example.com",
                                                    "is_superadmin": true,
                                                    "is_accountant": false
                                                },
                                                {
                                                    "id": 5,
                                                    "salutation_type": "female",
                                                    "firstname": "Alice",
                                                    "lastname": "Jones",
                                                    "email": "alice.jones@example.com",
                                                    "is_superadmin": false,
                                                    "is_accountant": true
                                                }
                                            ]
                                            """;

    /// <summary>
    ///     <c>UserService.GetAll</c> issues a <c>GET</c> request against <c>/3.0/users</c>
    ///     and deserializes the array of users into the canonical
    ///     <see cref="BexioApiNet.Abstractions.Models.MasterData.Users.User" /> records.
    /// </summary>
    [Test]
    public async Task UserService_GetAll_SendsGetRequest_DeserializesList()
    {
        Server
            .Given(Request.Create().WithPath(UsersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(UserListResponse));

        var service = new UserService(ConnectionHandler);

        var result = await service.GetAll(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(UsersPath));
            Assert.That(result.Data, Has.Count.EqualTo(2));
            Assert.That(result.Data![0].Id, Is.EqualTo(4));
            Assert.That(result.Data[0].SalutationType, Is.EqualTo("male"));
            Assert.That(result.Data[0].Firstname, Is.EqualTo("Rudolph"));
            Assert.That(result.Data[0].Lastname, Is.EqualTo("Smith"));
            Assert.That(result.Data[0].Email, Is.EqualTo("rudolph.smith@example.com"));
            Assert.That(result.Data[0].IsSuperadmin, Is.True);
            Assert.That(result.Data[0].IsAccountant, Is.False);
            Assert.That(result.Data[1].Id, Is.EqualTo(5));
            Assert.That(result.Data[1].SalutationType, Is.EqualTo("female"));
            Assert.That(result.Data[1].Firstname, Is.EqualTo("Alice"));
            Assert.That(result.Data[1].Lastname, Is.EqualTo("Jones"));
            Assert.That(result.Data[1].Email, Is.EqualTo("alice.jones@example.com"));
            Assert.That(result.Data[1].IsSuperadmin, Is.False);
            Assert.That(result.Data[1].IsAccountant, Is.True);
        });
    }

    /// <summary>
    ///     <c>UserService.GetAll</c> appends the supplied <see cref="QueryParameterUser" />
    ///     pagination values (<c>limit</c>, <c>offset</c>) to the URL.
    /// </summary>
    [Test]
    public async Task UserService_GetAll_WithQueryParameter_AppendsLimitAndOffset()
    {
        Server
            .Given(Request.Create().WithPath(UsersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new UserService(ConnectionHandler);

        var result = await service.GetAll(
            new QueryParameterUser(50, 100),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Url, Does.Contain("limit=50"));
            Assert.That(request.Url, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    ///     <c>UserService.GetMe</c> issues a <c>GET</c> against the singleton
    ///     <c>/3.0/users/me</c> route (no id segment) and surfaces the authenticated user's
    ///     full payload — including the optional admin-only <c>is_superadmin</c> / <c>is_accountant</c>
    ///     flags when present.
    /// </summary>
    [Test]
    public async Task UserService_GetMe_SendsGetRequestToMePath()
    {
        var expectedPath = $"{UsersPath}/me";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(UserResponse));

        var service = new UserService(ConnectionHandler);

        var result = await service.GetMe(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(4));
            Assert.That(result.Data.SalutationType, Is.EqualTo("male"));
            Assert.That(result.Data.Firstname, Is.EqualTo("Rudolph"));
            Assert.That(result.Data.Lastname, Is.EqualTo("Smith"));
            Assert.That(result.Data.Email, Is.EqualTo("rudolph.smith@example.com"));
            Assert.That(result.Data.IsSuperadmin, Is.True);
            Assert.That(result.Data.IsAccountant, Is.False);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>UserService.GetById</c> issues a <c>GET</c> request against
    ///     <c>/3.0/users/{user_id}</c> and surfaces the returned user on success.
    ///     Validates that admin-only fields are deserialized correctly when present.
    /// </summary>
    [Test]
    public async Task UserService_GetById_SendsGetRequestWithIdInPath()
    {
        const int id = 4;
        var expectedPath = $"{UsersPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(UserResponse));

        var service = new UserService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Email, Is.EqualTo("rudolph.smith@example.com"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    ///     <c>UserService.GetById</c> tolerates responses where the optional admin-only
    ///     fields (<c>is_superadmin</c>, <c>is_accountant</c>) are absent — these come back
    ///     as <see langword="null" /> and the email + id fields still deserialize.
    /// </summary>
    [Test]
    public async Task UserService_GetById_DeserializesUser_WithoutAdminFields()
    {
        const int id = 99;
        var expectedPath = $"{UsersPath}/{id}";

        const string payload = """
                               {
                                   "id": 99,
                                   "salutation_type": "female",
                                   "firstname": null,
                                   "lastname": null,
                                   "email": "no.admin@example.com"
                               }
                               """;

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(payload));

        var service = new UserService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(id));
            Assert.That(result.Data.Firstname, Is.Null);
            Assert.That(result.Data.Lastname, Is.Null);
            Assert.That(result.Data.IsSuperadmin, Is.Null);
            Assert.That(result.Data.IsAccountant, Is.Null);
            Assert.That(result.Data.Email, Is.EqualTo("no.admin@example.com"));
        });
    }
}
