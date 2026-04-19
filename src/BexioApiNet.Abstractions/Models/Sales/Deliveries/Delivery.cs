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

namespace BexioApiNet.Abstractions.Models.Sales.Deliveries;

/// <summary>
///     Delivery as returned by the Bexio <c>/2.0/kb_delivery</c> endpoint. Covers both the plain
///     <c>Delivery</c> schema returned by list and the <c>DeliveryWithDetails</c> variant returned by
///     show (which additionally populates <see cref="Positions" />).
///     <see href="https://docs.bexio.com/#tag/Deliveries/operation/v2ListDeliveries" />
/// </summary>
/// <param name="Id">Unique delivery identifier (read-only).</param>
/// <param name="DocumentNr">Delivery number (read-only, e.g. <c>LS-00001</c>).</param>
/// <param name="Title">Delivery title/subject.</param>
/// <param name="ContactId">References a contact object.</param>
/// <param name="ContactSubId">Optional additional contact reference (e.g. addressee within a company).</param>
/// <param name="UserId">References the user that owns the delivery.</param>
/// <param name="LogopaperId">References a logo-paper object.</param>
/// <param name="LanguageId">References a language object.</param>
/// <param name="BankAccountId">References a bank account object.</param>
/// <param name="CurrencyId">References a currency object.</param>
/// <param name="Header">Free-text header printed on top of the delivery.</param>
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
/// <param name="IsValidFrom">Delivery validity start in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="ContactAddress">Composed contact address string (read-only).</param>
/// <param name="DeliveryAddressType">
///     Delivery address selector: <c>0</c> use invoice address, <c>1</c> use custom delivery
///     address.
/// </param>
/// <param name="DeliveryAddress">Composed delivery address string (read-only).</param>
/// <param name="KbItemStatusId">Read-only delivery status id (10 Draft, 18 Done, 20 Canceled).</param>
/// <param name="ApiReference">Caller-supplied reference accessible only via the API.</param>
/// <param name="ViewedByClientAt">Timestamp when the delivery was first opened by the recipient (read-only).</param>
/// <param name="UpdatedAt">Timestamp of the last update in Bexio's <c>yyyy-MM-dd HH:mm:ss</c> format (read-only).</param>
/// <param name="Taxs">Read-only aggregated tax summary lines.</param>
/// <param name="Positions">
///     Optional polymorphic positions array populated on show responses. Kept as raw
///     <see cref="JsonElement" /> values since Bexio returns a union of several position types.
/// </param>
public sealed record Delivery(
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
    [property: JsonPropertyName("logopaper_id")]
    int? LogopaperId,
    [property: JsonPropertyName("language_id")]
    int? LanguageId,
    [property: JsonPropertyName("bank_account_id")]
    int? BankAccountId,
    [property: JsonPropertyName("currency_id")]
    int? CurrencyId,
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
    [property: JsonPropertyName("is_valid_from")]
    string? IsValidFrom,
    [property: JsonPropertyName("contact_address")]
    string? ContactAddress,
    [property: JsonPropertyName("delivery_address_type")]
    int? DeliveryAddressType,
    [property: JsonPropertyName("delivery_address")]
    string? DeliveryAddress,
    [property: JsonPropertyName("kb_item_status_id")]
    int? KbItemStatusId,
    [property: JsonPropertyName("api_reference")]
    string? ApiReference,
    [property: JsonPropertyName("viewed_by_client_at")]
    string? ViewedByClientAt,
    [property: JsonPropertyName("updated_at")]
    string? UpdatedAt,
    [property: JsonPropertyName("taxs")] IReadOnlyList<DeliveryTax>? Taxs,
    [property: JsonPropertyName("positions")]
    IReadOnlyList<JsonElement>? Positions
);
