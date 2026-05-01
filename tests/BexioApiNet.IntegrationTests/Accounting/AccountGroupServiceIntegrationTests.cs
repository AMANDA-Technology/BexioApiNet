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
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.IntegrationTests.Accounting;

/// <summary>
/// Integration tests covering the <see cref="AccountGroupService" /> entry points against
/// WireMock stubs. Verifies the path composed from <see cref="AccountGroupConfiguration" />
/// (<c>2.0/account_groups</c>) reaches the handler correctly and that the expected HTTP
/// verb is used.
/// </summary>
public sealed class AccountGroupServiceIntegrationTests : IntegrationTestBase
{
    private const string AccountGroupsPath = "/2.0/account_groups";

    /// <summary>
    /// Single account-group payload matching the <c>v2AccountGroup</c> schema in
    /// <c>doc/openapi/bexio-v3.json</c> for the <c>v2ListAccountGroups</c> operation.
    /// </summary>
    private const string AccountGroupResponse = """
                                                {
                                                    "id": 1,
                                                    "uuid": "5fe93c8a-b05f-4004-91f5-9177ffd011fd",
                                                    "account_no": "1",
                                                    "name": "Assets",
                                                    "parent_fibu_account_group_id": 3,
                                                    "is_active": true,
                                                    "is_locked": false
                                                }
                                                """;

    /// <summary>
    /// <c>AccountGroupService.Get()</c> must issue a <c>GET</c> request against
    /// <c>/2.0/account_groups</c> and return a successful <c>ApiResult</c> when the server
    /// returns an empty array.
    /// </summary>
    [Test]
    public async Task AccountGroupService_Get_SendsGetRequest()
    {
        Server
            .Given(Request.Create().WithPath(AccountGroupsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AccountGroupsPath));
        });
    }

    /// <summary>
    /// When the server returns a fully-populated account-group payload, the deserialized
    /// <c>AccountGroup</c> record must round-trip every field defined by the
    /// <c>v2AccountGroup</c> schema (id, uuid, account_no, name, parent_fibu_account_group_id,
    /// is_active, is_locked).
    /// </summary>
    [Test]
    public async Task AccountGroupService_Get_DeserializesAllSchemaFields()
    {
        Server
            .Given(Request.Create().WithPath(AccountGroupsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{AccountGroupResponse}]"));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var group = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(group.Id, Is.EqualTo(1));
            Assert.That(group.Uuid, Is.EqualTo("5fe93c8a-b05f-4004-91f5-9177ffd011fd"));
            Assert.That(group.AccountNo, Is.EqualTo("1"));
            Assert.That(group.Name, Is.EqualTo("Assets"));
            Assert.That(group.ParentFibuAccountGroupId, Is.EqualTo(3));
            Assert.That(group.IsActive, Is.True);
            Assert.That(group.IsLocked, Is.False);
        });
    }

    /// <summary>
    /// Top-level account groups have no parent group — the <c>parent_fibu_account_group_id</c>
    /// field is nullable per the spec, so a <c>null</c> value must round-trip as
    /// <see langword="null" /> on the deserialized record.
    /// </summary>
    [Test]
    public async Task AccountGroupService_Get_DeserializesNullParent()
    {
        const string topLevelGroup = """
                                     [{
                                         "id": 7,
                                         "uuid": "11111111-2222-3333-4444-555555555555",
                                         "account_no": "0",
                                         "name": "Root",
                                         "parent_fibu_account_group_id": null,
                                         "is_active": true,
                                         "is_locked": true
                                     }]
                                     """;

        Server
            .Given(Request.Create().WithPath(AccountGroupsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(topLevelGroup));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var group = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(group.ParentFibuAccountGroupId, Is.Null);
            Assert.That(group.IsLocked, Is.True);
        });
    }

    /// <summary>
    /// When a <see cref="QueryParameterAccountGroup"/> is supplied, <c>AccountGroupService.Get</c>
    /// must translate its <c>limit</c> and <c>offset</c> values into query-string parameters on the
    /// outgoing request.
    /// </summary>
    [Test]
    public async Task AccountGroupService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(AccountGroupsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new AccountGroupService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterAccountGroup(Limit: 50, Offset: 0),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AccountGroupsPath));
            Assert.That(request.RawQuery, Does.Contain("limit=50"));
            Assert.That(request.RawQuery, Does.Contain("offset=0"));
        });
    }
}
