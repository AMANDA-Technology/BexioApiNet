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

using BexioApiNet.Abstractions.Models.Sales.Positions;

namespace BexioApiNet.Abstractions.Models.Sales.Orders.Views;

/// <summary>
///     Create view for an order — body of <c>POST /2.0/kb_order</c>. Read-only fields (id, totals,
///     contact_address, delivery_address, kb_item_status_id, is_recurring, updated_at, taxs,
///     network_link, project_id, viewed_by_client_at) are intentionally omitted.
///     <see href="https://docs.bexio.com/#tag/Orders/operation/v2CreateOrder" />
/// </summary>
/// <param name="UserId">References the user that owns the order.</param>
/// <param name="DocumentNr">Order number. Must be omitted when automatic numbering is enabled.</param>
/// <param name="Title">Order title/subject.</param>
/// <param name="ContactId">References a contact object.</param>
/// <param name="ContactSubId">Optional additional contact reference (e.g. addressee within a company).</param>
/// <param name="PrProjectId">Write-only reference to a project object.</param>
/// <param name="LogopaperId">References a logo-paper object (deprecated by Bexio but still accepted).</param>
/// <param name="LanguageId">References a language object.</param>
/// <param name="BankAccountId">References a bank account object.</param>
/// <param name="CurrencyId">References a currency object.</param>
/// <param name="PaymentTypeId">References a payment type object.</param>
/// <param name="Header">Free-text header printed on top of the order.</param>
/// <param name="Footer">Free-text footer printed below the positions.</param>
/// <param name="MwstType">VAT/MwSt. handling flag: <c>0</c> including taxes, <c>1</c> excluding taxes, <c>2</c> exempt.</param>
/// <param name="MwstIsNet">
///     When <see langword="true" /> and <see cref="MwstType" /> is <c>0</c>, totals are interpreted as
///     net.
/// </param>
/// <param name="ShowPositionTaxes">When <see langword="true" />, per-position taxes are shown on the printed document.</param>
/// <param name="IsValidFrom">Order validity start in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="ContactAddressManual">Manually overridden contact address (write-only).</param>
/// <param name="DeliveryAddressType">
///     Delivery address selector: <c>0</c> use invoice address, <c>1</c> use custom delivery
///     address.
/// </param>
/// <param name="DeliveryAddressManual">
///     Manually overridden delivery address (write-only, used when
///     <see cref="DeliveryAddressType" /> is <c>1</c>).
/// </param>
/// <param name="ApiReference">Caller-supplied reference accessible only via the API.</param>
/// <param name="TemplateSlug">References a document template slug.</param>
/// <param name="Positions">
///     Polymorphic list of positions to create. Pass any mix of <see cref="Position" /> subtypes — the
///     converter emits the <c>type</c> discriminator expected by Bexio.
/// </param>
public sealed record OrderCreate(
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("document_nr")]
    string? DocumentNr = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("contact_id")]
    int? ContactId = null,
    [property: JsonPropertyName("contact_sub_id")]
    int? ContactSubId = null,
    [property: JsonPropertyName("pr_project_id")]
    int? PrProjectId = null,
    [property: JsonPropertyName("logopaper_id")]
    int? LogopaperId = null,
    [property: JsonPropertyName("language_id")]
    int? LanguageId = null,
    [property: JsonPropertyName("bank_account_id")]
    int? BankAccountId = null,
    [property: JsonPropertyName("currency_id")]
    int? CurrencyId = null,
    [property: JsonPropertyName("payment_type_id")]
    int? PaymentTypeId = null,
    [property: JsonPropertyName("header")] string? Header = null,
    [property: JsonPropertyName("footer")] string? Footer = null,
    [property: JsonPropertyName("mwst_type")]
    int? MwstType = null,
    [property: JsonPropertyName("mwst_is_net")]
    bool? MwstIsNet = null,
    [property: JsonPropertyName("show_position_taxes")]
    bool? ShowPositionTaxes = null,
    [property: JsonPropertyName("is_valid_from")]
    string? IsValidFrom = null,
    [property: JsonPropertyName("contact_address_manual")]
    string? ContactAddressManual = null,
    [property: JsonPropertyName("delivery_address_type")]
    int? DeliveryAddressType = null,
    [property: JsonPropertyName("delivery_address_manual")]
    string? DeliveryAddressManual = null,
    [property: JsonPropertyName("api_reference")]
    string? ApiReference = null,
    [property: JsonPropertyName("template_slug")]
    string? TemplateSlug = null,
    [property: JsonPropertyName("positions")]
    IReadOnlyList<Position>? Positions = null
);