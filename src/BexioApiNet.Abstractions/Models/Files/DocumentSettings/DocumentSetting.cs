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

namespace BexioApiNet.Abstractions.Models.Files.DocumentSettings;

/// <summary>
/// Document setting (a.k.a. <c>KbItemSetting</c>) returned by the Bexio v2.0
/// <c>/kb_item_setting</c> endpoint. Describes the default configuration applied to a
/// given document class (offer, order, invoice, delivery, credit voucher, ...).
/// <see href="https://docs.bexio.com/#tag/Document-Settings/operation/v2ListDocumentSettings">v2 List Document Settings</see>
/// </summary>
/// <param name="Id">Unique document-setting identifier.</param>
/// <param name="Text">Display name of the document setting (e.g. <c>Quote</c>).</param>
/// <param name="KbItemClass">Target document class (e.g. <c>KbOffer</c>, <c>KbInvoice</c>).</param>
/// <param name="EnumerationFormat">Format string used to build the document number (e.g. <c>AN-%nummer%</c>).</param>
/// <param name="UseAutomaticEnumeration">When true, document numbers are auto-generated.</param>
/// <param name="UseYearlyEnumeration">When true, the document counter resets at the start of every year.</param>
/// <param name="NextNr">The next number that will be assigned to a new document.</param>
/// <param name="NrMinLength">Minimum length of the numeric part of the document number (zero-padded).</param>
/// <param name="DefaultTimePeriodInDays">Default validity / due period applied to new documents, in days.</param>
/// <param name="DefaultLogopaperId">Default logopaper template id applied to new documents.</param>
/// <param name="DefaultLanguageId">Default language id applied to new documents.</param>
/// <param name="DefaultClientBankAccountNewId">Default client bank account id applied to new documents.</param>
/// <param name="DefaultCurrencyId">Default currency id applied to new documents.</param>
/// <param name="DefaultMwstType">Default VAT type applied to new documents.</param>
/// <param name="DefaultMwstIsNet">When true, prices on new documents are entered net of VAT.</param>
/// <param name="DefaultNbDecimalsAmount">Default number of decimal places for amounts.</param>
/// <param name="DefaultNbDecimalsPrice">Default number of decimal places for unit prices.</param>
/// <param name="DefaultShowPositionTaxes">When true, per-position taxes are shown on new documents.</param>
/// <param name="DefaultTitle">Default document title (e.g. <c>Angebot</c>).</param>
/// <param name="DefaultShowEsrOnSamePage">When true, the ESR payment slip is printed on the same page as the document.</param>
/// <param name="DefaultPaymentTypeId">Default payment type id applied to new documents.</param>
/// <param name="KbTermsOfPaymentTemplateId">Optional default terms-of-payment template id (nullable).</param>
/// <param name="DefaultShowTotal">When true, the document total is shown on new documents.</param>
public sealed record DocumentSetting(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("kb_item_class")] string KbItemClass,
    [property: JsonPropertyName("enumeration_format")] string EnumerationFormat,
    [property: JsonPropertyName("use_automatic_enumeration")] bool UseAutomaticEnumeration,
    [property: JsonPropertyName("use_yearly_enumeration")] bool UseYearlyEnumeration,
    [property: JsonPropertyName("next_nr")] int NextNr,
    [property: JsonPropertyName("nr_min_length")] int NrMinLength,
    [property: JsonPropertyName("default_time_period_in_days")] int DefaultTimePeriodInDays,
    [property: JsonPropertyName("default_logopaper_id")] int DefaultLogopaperId,
    [property: JsonPropertyName("default_language_id")] int DefaultLanguageId,
    [property: JsonPropertyName("default_client_bank_account_new_id")] int DefaultClientBankAccountNewId,
    [property: JsonPropertyName("default_currency_id")] int DefaultCurrencyId,
    [property: JsonPropertyName("default_mwst_type")] int DefaultMwstType,
    [property: JsonPropertyName("default_mwst_is_net")] bool DefaultMwstIsNet,
    [property: JsonPropertyName("default_nb_decimals_amount")] int DefaultNbDecimalsAmount,
    [property: JsonPropertyName("default_nb_decimals_price")] int DefaultNbDecimalsPrice,
    [property: JsonPropertyName("default_show_position_taxes")] bool DefaultShowPositionTaxes,
    [property: JsonPropertyName("default_title")] string DefaultTitle,
    [property: JsonPropertyName("default_show_esr_on_same_page")] bool DefaultShowEsrOnSamePage,
    [property: JsonPropertyName("default_payment_type_id")] int DefaultPaymentTypeId,
    [property: JsonPropertyName("kb_terms_of_payment_template_id")] int? KbTermsOfPaymentTemplateId,
    [property: JsonPropertyName("default_show_total")] bool DefaultShowTotal
);
