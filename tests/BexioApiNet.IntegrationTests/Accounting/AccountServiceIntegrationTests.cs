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
/// Integration tests verifying <see cref="AccountService"/> drives the real
/// <see cref="BexioConnectionHandler"/> with the expected HTTP verb, request path and
/// query string. Each test arranges a WireMock stub, exercises the service and asserts
/// on the recorded request in <see cref="WireMock.Server.WireMockServer.LogEntries"/>.
/// </summary>
public sealed class AccountServiceIntegrationTests : IntegrationTestBase
{
    private const string AccountsPath = "/2.0/accounts";

    /// <summary>
    /// Single account payload matching the <c>v2Account</c> schema in
    /// <c>doc/openapi/bexio-v3.json</c> for the <c>v2ListAccounts</c> operation.
    /// </summary>
    private const string AccountResponse = """
                                           {
                                               "id": 1,
                                               "uuid": "c7da5b70-2d27-467e-abd9-9c3ac0f83c7d",
                                               "account_no": "3201",
                                               "name": "Gross proceeds credit sales",
                                               "account_type": 1,
                                               "tax_id": 40,
                                               "fibu_account_group_id": 65,
                                               "is_active": true,
                                               "is_locked": false
                                           }
                                           """;

    /// <summary>
    /// <c>AccountService.Get()</c> must issue a single <c>GET</c> request to
    /// <c>/2.0/accounts</c> and surface a successful <c>ApiResult</c> when the server replies
    /// with an empty array.
    /// </summary>
    [Test]
    public async Task AccountService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new AccountService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AccountsPath));
        });
    }

    /// <summary>
    /// When the server returns a fully-populated account payload, the deserialized
    /// <c>Account</c> record must round-trip every field defined by the
    /// <c>v2Account</c> schema (id, uuid, account_no, name, account_type, tax_id,
    /// fibu_account_group_id, is_active, is_locked).
    /// </summary>
    [Test]
    public async Task AccountService_Get_DeserializesAllSchemaFields()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{AccountResponse}]"));

        var service = new AccountService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);

        var account = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.EqualTo(1));
            Assert.That(account.Uuid, Is.EqualTo("c7da5b70-2d27-467e-abd9-9c3ac0f83c7d"));
            Assert.That(account.AccountNo, Is.EqualTo("3201"));
            Assert.That(account.Name, Is.EqualTo("Gross proceeds credit sales"));
            Assert.That(account.AccountType, Is.EqualTo(1));
            Assert.That(account.TaxId, Is.EqualTo(40));
            Assert.That(account.FibuAccountGroupId, Is.EqualTo(65));
            Assert.That(account.IsActive, Is.True);
            Assert.That(account.IsLocked, Is.False);
        });
    }

    /// <summary>
    /// When Bexio omits <c>tax_id</c> the field is nullable per the spec — the
    /// deserialized <c>Account.TaxId</c> must therefore round-trip as <see langword="null" />.
    /// </summary>
    [Test]
    public async Task AccountService_Get_DeserializesNullTaxId()
    {
        const string responseWithNullTax = """
                                           [{
                                               "id": 2,
                                               "uuid": "5fe93c8a-b05f-4004-91f5-9177ffd011fd",
                                               "account_no": "1000",
                                               "name": "Cash",
                                               "account_type": 3,
                                               "tax_id": null,
                                               "fibu_account_group_id": 1,
                                               "is_active": true,
                                               "is_locked": false
                                           }]
                                           """;

        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(responseWithNullTax));

        var service = new AccountService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null.And.Not.Empty);
        Assert.That(result.Data![0].TaxId, Is.Null);
    }

    /// <summary>
    /// When a <see cref="QueryParameterAccount"/> is supplied, <c>AccountService.Get</c> must
    /// translate its <c>limit</c> and <c>offset</c> values into query-string parameters on the
    /// outgoing request.
    /// </summary>
    [Test]
    public async Task AccountService_Get_WithQueryParams_AppendsParams()
    {
        Server
            .Given(Request.Create().WithPath(AccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new AccountService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterAccount(Limit: 25, Offset: 100),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(AccountsPath));
            Assert.That(request.RawQuery, Does.Contain("limit=25"));
            Assert.That(request.RawQuery, Does.Contain("offset=100"));
        });
    }
}
