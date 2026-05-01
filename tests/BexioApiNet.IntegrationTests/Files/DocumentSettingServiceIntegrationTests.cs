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

using BexioApiNet.Services.Connectors.Files;

namespace BexioApiNet.IntegrationTests.Files;

/// <summary>
///     Integration tests covering <see cref="DocumentSettingService" />. The request path is
///     composed from <see cref="DocumentSettingConfiguration" /> (<c>2.0/kb_item_setting</c>) and
///     must reach WireMock intact when the service is driven through the real connection handler.
///     The response body matches the v2 Bexio OpenAPI <c>KbItemSetting</c> schema exactly so
///     deserialization of every documented field can be asserted end-to-end (including the
///     nullable <c>kb_terms_of_payment_template_id</c>).
/// </summary>
public sealed class DocumentSettingServiceIntegrationTests : IntegrationTestBase
{
    private const string DocumentSettingsPath = "/2.0/kb_item_setting";

    /// <summary>
    ///     Fully populated v2 Bexio <c>KbItemSetting</c> response body matching the OpenAPI schema
    ///     example values verbatim. Designed to exercise the deserializer on every documented
    ///     field, including the only nullable property (<c>kb_terms_of_payment_template_id</c>)
    ///     and every snake_case <c>default_*</c> default-value field.
    /// </summary>
    private const string DocumentSettingResponse = """
                                                   {
                                                       "id": 1,
                                                       "text": "Quote",
                                                       "kb_item_class": "KbOffer",
                                                       "enumeration_format": "AN-%nummer%",
                                                       "use_automatic_enumeration": true,
                                                       "use_yearly_enumeration": false,
                                                       "next_nr": 1,
                                                       "nr_min_length": 5,
                                                       "default_time_period_in_days": 14,
                                                       "default_logopaper_id": 1,
                                                       "default_language_id": 1,
                                                       "default_client_bank_account_new_id": 1,
                                                       "default_currency_id": 1,
                                                       "default_mwst_type": 0,
                                                       "default_mwst_is_net": true,
                                                       "default_nb_decimals_amount": 2,
                                                       "default_nb_decimals_price": 2,
                                                       "default_show_position_taxes": false,
                                                       "default_title": "Angebot",
                                                       "default_show_esr_on_same_page": false,
                                                       "default_payment_type_id": 1,
                                                       "kb_terms_of_payment_template_id": 1,
                                                       "default_show_total": true
                                                   }
                                                   """;

    /// <summary>
    ///     <c>DocumentSettingService.Get()</c> must issue a <c>GET</c> against
    ///     <c>/2.0/kb_item_setting</c> and deserialize every documented field of the
    ///     <c>KbItemSetting</c> schema (22 properties).
    /// </summary>
    [Test]
    public async Task DocumentSettingService_Get_SendsGetRequestToCorrectPath()
    {
        Server
            .Given(Request.Create().WithPath(DocumentSettingsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody($"[{DocumentSettingResponse}]"));

        var service = new DocumentSettingService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;
        var setting = result.Data!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(DocumentSettingsPath));
            Assert.That(setting.Id, Is.EqualTo(1));
            Assert.That(setting.Text, Is.EqualTo("Quote"));
            Assert.That(setting.KbItemClass, Is.EqualTo("KbOffer"));
            Assert.That(setting.EnumerationFormat, Is.EqualTo("AN-%nummer%"));
            Assert.That(setting.UseAutomaticEnumeration, Is.True);
            Assert.That(setting.UseYearlyEnumeration, Is.False);
            Assert.That(setting.NextNr, Is.EqualTo(1));
            Assert.That(setting.NrMinLength, Is.EqualTo(5));
            Assert.That(setting.DefaultTimePeriodInDays, Is.EqualTo(14));
            Assert.That(setting.DefaultLogopaperId, Is.EqualTo(1));
            Assert.That(setting.DefaultLanguageId, Is.EqualTo(1));
            Assert.That(setting.DefaultClientBankAccountNewId, Is.EqualTo(1));
            Assert.That(setting.DefaultCurrencyId, Is.EqualTo(1));
            Assert.That(setting.DefaultMwstType, Is.EqualTo(0));
            Assert.That(setting.DefaultMwstIsNet, Is.True);
            Assert.That(setting.DefaultNbDecimalsAmount, Is.EqualTo(2));
            Assert.That(setting.DefaultNbDecimalsPrice, Is.EqualTo(2));
            Assert.That(setting.DefaultShowPositionTaxes, Is.False);
            Assert.That(setting.DefaultTitle, Is.EqualTo("Angebot"));
            Assert.That(setting.DefaultShowEsrOnSamePage, Is.False);
            Assert.That(setting.DefaultPaymentTypeId, Is.EqualTo(1));
            Assert.That(setting.KbTermsOfPaymentTemplateId, Is.EqualTo(1));
            Assert.That(setting.DefaultShowTotal, Is.True);
        });
    }

    /// <summary>
    ///     The OpenAPI spec marks <c>kb_terms_of_payment_template_id</c> as the only nullable
    ///     property on <c>KbItemSetting</c>. When Bexio returns <c>null</c> for it, the model
    ///     must deserialize as <see langword="null" /> on
    ///     <see cref="BexioApiNet.Abstractions.Models.Files.DocumentSettings.DocumentSetting.KbTermsOfPaymentTemplateId" />.
    /// </summary>
    [Test]
    public async Task DocumentSettingService_Get_DeserializesNullableTermsOfPaymentTemplateId()
    {
        const string body = """
                            [
                                {
                                    "id": 2,
                                    "text": "Invoice",
                                    "kb_item_class": "KbInvoice",
                                    "enumeration_format": "RE-%nummer%",
                                    "use_automatic_enumeration": true,
                                    "use_yearly_enumeration": true,
                                    "next_nr": 1,
                                    "nr_min_length": 4,
                                    "default_time_period_in_days": 30,
                                    "default_logopaper_id": 2,
                                    "default_language_id": 2,
                                    "default_client_bank_account_new_id": 2,
                                    "default_currency_id": 2,
                                    "default_mwst_type": 1,
                                    "default_mwst_is_net": false,
                                    "default_nb_decimals_amount": 2,
                                    "default_nb_decimals_price": 4,
                                    "default_show_position_taxes": true,
                                    "default_title": "Rechnung",
                                    "default_show_esr_on_same_page": true,
                                    "default_payment_type_id": 2,
                                    "kb_terms_of_payment_template_id": null,
                                    "default_show_total": true
                                }
                            ]
                            """;

        Server
            .Given(Request.Create().WithPath(DocumentSettingsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(body));

        var service = new DocumentSettingService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);
        var setting = result.Data!.Single();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(setting.KbTermsOfPaymentTemplateId, Is.Null);
            Assert.That(setting.UseYearlyEnumeration, Is.True);
            Assert.That(setting.DefaultMwstIsNet, Is.False);
            Assert.That(setting.DefaultShowEsrOnSamePage, Is.True);
        });
    }

    /// <summary>
    ///     When Bexio responds with an empty array, <c>DocumentSettingService.Get()</c> must
    ///     surface an empty collection on a successful <c>ApiResult</c>.
    /// </summary>
    [Test]
    public async Task DocumentSettingService_Get_WithEmptyResponse_ReturnsEmptyCollection()
    {
        Server
            .Given(Request.Create().WithPath(DocumentSettingsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("[]"));

        var service = new DocumentSettingService(ConnectionHandler);

        var result = await service.Get(TestContext.CurrentContext.CancellationToken);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data, Is.Empty);
        });
    }
}
