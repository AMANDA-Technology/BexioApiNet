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

using System.Text.Json;

namespace BexioApiNet.Abstractions.Models.Sales.Orders;

/// <summary>
///     Order as returned by the Bexio <c>/2.0/kb_order</c> endpoint. Covers both the plain
///     <c>Order</c> schema returned by list and the <c>OrderWithDetails</c> variant returned by
///     show/create/update (which additionally populates <see cref="Positions" />).
///     <see href="https://docs.bexio.com/#tag/Orders/operation/v2ListOrders" />
/// </summary>
/// <param name="Id">Unique order identifier (read-only).</param>
/// <param name="DocumentNr">Order number. Must be omitted when automatic numbering is enabled, required otherwise.</param>
/// <param name="Title">Order title/subject.</param>
/// <param name="ContactId">References a contact object.</param>
/// <param name="ContactSubId">Optional additional contact reference (e.g. addressee within a company).</param>
/// <param name="UserId">References the user that owns the order.</param>
/// <param name="ProjectId">Read-only reference to a project object.</param>
/// <param name="PrProjectId">Write-only reference to a project object, accepted on create/update.</param>
/// <param name="LogopaperId">References a logo-paper object (deprecated by Bexio but still accepted).</param>
/// <param name="LanguageId">References a language object.</param>
/// <param name="BankAccountId">References a bank account object.</param>
/// <param name="CurrencyId">References a currency object.</param>
/// <param name="PaymentTypeId">References a payment type object.</param>
/// <param name="Header">Free-text header printed on top of the order.</param>
/// <param name="Footer">Free-text footer printed below the positions.</param>
/// <param name="TotalGross">Gross total as a formatted decimal string (read-only).</param>
/// <param name="TotalNet">Net total as a formatted decimal string (read-only).</param>
/// <param name="TotalTaxes">Total of applicable taxes as a formatted decimal string (read-only).</param>
/// <param name="Total">Grand total as a formatted decimal string (read-only).</param>
/// <param name="TotalRoundingDifference">Rounding difference applied to the total (read-only).</param>
/// <param name="MwstType">VAT/MwSt. handling flag: <c>0</c> including taxes, <c>1</c> excluding taxes, <c>2</c> exempt.</param>
/// <param name="MwstIsNet">
///     When <see langword="true" /> and <see cref="MwstType" /> is <c>0</c>, totals are interpreted as
///     net.
/// </param>
/// <param name="ShowPositionTaxes">When <see langword="true" />, per-position taxes are shown on the printed document.</param>
/// <param name="IsValidFrom">Order validity start in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="ContactAddress">Composed contact address string (read-only).</param>
/// <param name="ContactAddressManual">Manually overridden contact address (write-only).</param>
/// <param name="DeliveryAddressType">
///     Delivery address selector: <c>0</c> use invoice address, <c>1</c> use custom delivery
///     address.
/// </param>
/// <param name="DeliveryAddress">Composed delivery address string (read-only).</param>
/// <param name="DeliveryAddressManual">
///     Manually overridden delivery address (write-only, used when
///     <see cref="DeliveryAddressType" /> is <c>1</c>).
/// </param>
/// <param name="KbItemStatusId">Read-only order status id (5 Pending, 6 Done, 15 Partial, 21 Canceled).</param>
/// <param name="IsRecurring">When <see langword="true" />, the order has a repetition schedule attached (read-only).</param>
/// <param name="ApiReference">Caller-supplied reference accessible only via the API.</param>
/// <param name="ViewedByClientAt">Timestamp when the order was first opened by the recipient (read-only).</param>
/// <param name="UpdatedAt">Timestamp of the last update in Bexio's <c>yyyy-MM-dd HH:mm:ss</c> format (read-only).</param>
/// <param name="TemplateSlug">References a document template slug.</param>
/// <param name="Taxs">Read-only aggregated tax summary lines.</param>
/// <param name="NetworkLink">Read-only network link used by the Bexio document viewer.</param>
/// <param name="Positions">
///     Optional polymorphic positions array populated on show/create/update responses. Kept as raw
///     <see cref="JsonElement" /> values since Bexio returns a union of several position types.
/// </param>
public sealed record Order(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("document_nr")]
    string? DocumentNr,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("contact_id")]
    int? ContactId,
    [property: JsonPropertyName("contact_sub_id")]
    int? ContactSubId,
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("project_id")]
    int? ProjectId,
    [property: JsonPropertyName("pr_project_id")]
    int? PrProjectId,
    [property: JsonPropertyName("logopaper_id")]
    int? LogopaperId,
    [property: JsonPropertyName("language_id")]
    int? LanguageId,
    [property: JsonPropertyName("bank_account_id")]
    int? BankAccountId,
    [property: JsonPropertyName("currency_id")]
    int? CurrencyId,
    [property: JsonPropertyName("payment_type_id")]
    int? PaymentTypeId,
    [property: JsonPropertyName("header")] string? Header,
    [property: JsonPropertyName("footer")] string? Footer,
    [property: JsonPropertyName("total_gross")]
    string? TotalGross,
    [property: JsonPropertyName("total_net")]
    string? TotalNet,
    [property: JsonPropertyName("total_taxes")]
    string? TotalTaxes,
    [property: JsonPropertyName("total")] string? Total,
    [property: JsonPropertyName("total_rounding_difference")]
    decimal? TotalRoundingDifference,
    [property: JsonPropertyName("mwst_type")]
    int? MwstType,
    [property: JsonPropertyName("mwst_is_net")]
    bool? MwstIsNet,
    [property: JsonPropertyName("show_position_taxes")]
    bool? ShowPositionTaxes,
    [property: JsonPropertyName("is_valid_from")]
    string? IsValidFrom,
    [property: JsonPropertyName("contact_address")]
    string? ContactAddress,
    [property: JsonPropertyName("contact_address_manual")]
    string? ContactAddressManual,
    [property: JsonPropertyName("delivery_address_type")]
    int? DeliveryAddressType,
    [property: JsonPropertyName("delivery_address")]
    string? DeliveryAddress,
    [property: JsonPropertyName("delivery_address_manual")]
    string? DeliveryAddressManual,
    [property: JsonPropertyName("kb_item_status_id")]
    int? KbItemStatusId,
    [property: JsonPropertyName("is_recurring")]
    bool? IsRecurring,
    [property: JsonPropertyName("api_reference")]
    string? ApiReference,
    [property: JsonPropertyName("viewed_by_client_at")]
    string? ViewedByClientAt,
    [property: JsonPropertyName("updated_at")]
    string? UpdatedAt,
    [property: JsonPropertyName("template_slug")]
    string? TemplateSlug,
    [property: JsonPropertyName("taxs")] IReadOnlyList<OrderTax>? Taxs,
    [property: JsonPropertyName("network_link")]
    string? NetworkLink,
    [property: JsonPropertyName("positions")]
    IReadOnlyList<JsonElement>? Positions
);