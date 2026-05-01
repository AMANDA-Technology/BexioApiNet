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

using BexioApiNet.Abstractions.Models.Banking.BankAccounts.Enums;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.IntegrationTests.Banking;

/// <summary>
/// Integration tests covering <see cref="BankAccountService"/>. The request path is composed from
/// <see cref="BankingConfiguration"/> (<c>3.0/banking/accounts</c>) and must reach WireMock
/// intact when the service is driven through the real connection handler. Tests use
/// fully-populated JSON payloads matching the OpenAPI v3.0.0 schema and assert end-to-end
/// deserialisation of every field.
/// </summary>
public sealed class BankAccountServiceIntegrationTests : IntegrationTestBase
{
    private const string BankAccountsPath = "/3.0/banking/accounts";

    /// <summary>
    /// Schema-accurate JSON payload for a single bank account, populated with values for
    /// every field defined by the Bexio v3.0 OpenAPI schema for
    /// <c>GET /3.0/banking/accounts/{bank_account_id}</c>.
    /// </summary>
    private const string BankAccountJson = """
                                           {
                                               "id": 42,
                                               "name": "Main Account",
                                               "owner": "Acme AG",
                                               "owner_address": "Bahnhofstrasse",
                                               "owner_house_number": "1",
                                               "owner_zip": "8001",
                                               "owner_city": "Zurich",
                                               "owner_country_code": "CH",
                                               "bc_nr": "9000",
                                               "bank_name": "PostFinance",
                                               "bank_nr": "9000",
                                               "bank_account_nr": "12-345678-9",
                                               "iban_nr": "CH9300762011623852957",
                                               "currency_id": 1,
                                               "account_id": 100,
                                               "remarks": "Primary operating account",
                                               "qr_invoice_iban": "CH4431999123000889012",
                                               "invoice_mode": "qr_iban",
                                               "is_esr": false,
                                               "esr_besr_id": "01-12345-6",
                                               "esr_post_account_nr": "01-12345-6",
                                               "esr_payment_for_text": "Acme AG",
                                               "esr_in_favour_of_text": "Acme AG, 8001 Zurich",
                                               "type": "bank"
                                           }
                                           """;

    /// <summary>
    /// <c>BankAccountService.Get()</c> must issue a <c>GET</c> against
    /// <c>/3.0/banking/accounts</c>, deserialise an array of fully-populated bank account
    /// JSON objects into <c>List&lt;BankAccountGet&gt;</c>, and surface every field of the
    /// schema with the correct C# type.
    /// </summary>
    [Test]
    public async Task BankAccountService_Get_DeserialisesAllSchemaFields()
    {
        Server
            .Given(Request.Create().WithPath(BankAccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{BankAccountJson}]"));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Has.Count.EqualTo(1));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BankAccountsPath));
        });

        var account = result.Data![0];
        Assert.Multiple(() =>
        {
            Assert.That(account.Id, Is.EqualTo(42));
            Assert.That(account.Name, Is.EqualTo("Main Account"));
            Assert.That(account.Owner, Is.EqualTo("Acme AG"));
            Assert.That(account.OwnerAddress, Is.EqualTo("Bahnhofstrasse"));
            Assert.That(account.OwnerHouseNumber, Is.EqualTo("1"));
            Assert.That(account.OwnerZip, Is.EqualTo("8001"));
            Assert.That(account.OwnerCity, Is.EqualTo("Zurich"));
            Assert.That(account.OwnerCountryCode, Is.EqualTo("CH"));
            Assert.That(account.BcNr, Is.EqualTo("9000"));
            Assert.That(account.BankName, Is.EqualTo("PostFinance"));
            Assert.That(account.BankNr, Is.EqualTo("9000"));
            Assert.That(account.BankAccountNr, Is.EqualTo("12-345678-9"));
            Assert.That(account.IbanNr, Is.EqualTo("CH9300762011623852957"));
            Assert.That(account.CurrencyId, Is.EqualTo(1));
            Assert.That(account.AccountId, Is.EqualTo(100));
            Assert.That(account.Remarks, Is.EqualTo("Primary operating account"));
            Assert.That(account.QrInvoiceIban, Is.EqualTo("CH4431999123000889012"));
            Assert.That(account.InvoiceMode, Is.EqualTo(BankAccountInvoiceMode.qr_iban));
            Assert.That(account.IsEsr, Is.False);
            Assert.That(account.EsrBesrId, Is.EqualTo("01-12345-6"));
            Assert.That(account.EsrPostAccountNr, Is.EqualTo("01-12345-6"));
            Assert.That(account.EsrPaymentForText, Is.EqualTo("Acme AG"));
            Assert.That(account.EsrInFavourOfText, Is.EqualTo("Acme AG, 8001 Zurich"));
            Assert.That(account.Type, Is.EqualTo("bank"));
        });
    }

    /// <summary>
    /// <c>BankAccountService.Get</c> with a <see cref="QueryParameterBankAccount"/> must
    /// forward both <c>limit</c> and <c>offset</c> to WireMock as URL query parameters with
    /// the values supplied by the caller.
    /// </summary>
    [Test]
    public async Task BankAccountService_Get_WithQueryParameter_PassesLimitAndOffsetOnUrl()
    {
        Server
            .Given(Request.Create().WithPath(BankAccountsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterBankAccount(Limit: 50, Offset: 100),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BankAccountsPath));
            Assert.That(request.Url, Does.Contain("limit=50"));
            Assert.That(request.Url, Does.Contain("offset=100"));
        });
    }

    /// <summary>
    /// <c>BankAccountService.GetById</c> must issue a <c>GET</c> against
    /// <c>/3.0/banking/accounts/{id}</c> and deserialise every field of the
    /// fully-populated single-object response per the Bexio v3.0 spec.
    /// </summary>
    [Test]
    public async Task BankAccountService_GetById_DeserialisesAllSchemaFields()
    {
        const int id = 42;
        var expectedPath = $"{BankAccountsPath}/{id}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(BankAccountJson));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Id, Is.EqualTo(42));
            Assert.That(result.Data.Name, Is.EqualTo("Main Account"));
            Assert.That(result.Data.IbanNr, Is.EqualTo("CH9300762011623852957"));
            Assert.That(result.Data.InvoiceMode, Is.EqualTo(BankAccountInvoiceMode.qr_iban));
            Assert.That(result.Data.IsEsr, Is.False);
            Assert.That(result.Data.OwnerCountryCode, Is.EqualTo("CH"));
        });
    }

    /// <summary>
    /// Each enum value of <see cref="BankAccountInvoiceMode"/> must round-trip through the
    /// Bexio JSON payload using the lowercase Bexio-defined string. This catches a missing
    /// converter or accidental enum reordering.
    /// </summary>
    [TestCase("none", BankAccountInvoiceMode.none)]
    [TestCase("qr_iban", BankAccountInvoiceMode.qr_iban)]
    [TestCase("iban_with_creditor_reference", BankAccountInvoiceMode.iban_with_creditor_reference)]
    [TestCase("iban_only", BankAccountInvoiceMode.iban_only)]
    public async Task BankAccountService_GetById_ParsesAllInvoiceModeEnumValues(
        string jsonValue,
        BankAccountInvoiceMode expected)
    {
        const int id = 1;
        var json = $$"""
                     {
                         "id": 1,
                         "name": "Test",
                         "invoice_mode": "{{jsonValue}}"
                     }
                     """;

        Server
            .Given(Request.Create().WithPath($"{BankAccountsPath}/{id}").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(json));

        var service = new BankAccountService(ConnectionHandler);

        var result = await service.GetById(id, TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.InvoiceMode, Is.EqualTo(expected));
        });
    }
}
