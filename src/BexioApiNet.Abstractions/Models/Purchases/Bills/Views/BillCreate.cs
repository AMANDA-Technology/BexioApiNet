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

namespace BexioApiNet.Abstractions.Models.Purchases.Bills.Views;

/// <summary>
/// Create view for <c>POST /4.0/purchase/bills</c>. All required fields per the Bexio
/// API spec are non-nullable positional parameters; optional fields default to
/// <see langword="null"/> and are only serialized when supplied.
/// <see href="https://docs.bexio.com/#tag/Bills">Bills</see>
/// </summary>
/// <param name="SupplierId">Identifier of the supplier contact.</param>
/// <param name="ContactPartnerId">Identifier of the Bexio contact partner.</param>
/// <param name="CurrencyCode">ISO currency code of the bill.</param>
/// <param name="Address">Address block describing the supplier.</param>
/// <param name="BillDate">Bill issue date.</param>
/// <param name="DueDate">Due date for payment.</param>
/// <param name="ManualAmount">Whether <c>AmountMan</c> (true) or <c>AmountCalc</c> (false) is the authoritative bill amount.</param>
/// <param name="ItemNet">Whether line-item amounts are net (<see langword="true"/>) or gross (<see langword="false"/>).</param>
/// <param name="LineItems">Bill line items (1-100).</param>
/// <param name="Discounts">Discounts applied to the bill (0-100).</param>
/// <param name="AttachmentIds">Bexio file identifiers to attach to the bill.</param>
/// <param name="VendorRef">Vendor reference text.</param>
/// <param name="Title">Free-form bill title.</param>
/// <param name="AmountMan">Manual amount. Required when <see cref="ManualAmount"/> is <see langword="true"/>.</param>
/// <param name="AmountCalc">Calculated amount. Required when <see cref="ManualAmount"/> is <see langword="false"/>.</param>
/// <param name="ExchangeRate">Exchange rate. Required when <see cref="CurrencyCode"/> differs from the base currency.</param>
/// <param name="BaseCurrencyAmount">Bill amount expressed in the base currency. Required with <see cref="ExchangeRate"/>.</param>
/// <param name="PurchaseOrderId">Optional purchase-order identifier this bill originates from.</param>
/// <param name="QrBillInformation">QR bill payload string when the bill was captured from a QR invoice.</param>
/// <param name="Payment">Optional payment block attached to the bill.</param>
public sealed record BillCreate(
    [property: JsonPropertyName("supplier_id")] int SupplierId,
    [property: JsonPropertyName("contact_partner_id")] int ContactPartnerId,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("address")] BillAddress Address,
    [property: JsonPropertyName("bill_date")] DateOnly BillDate,
    [property: JsonPropertyName("due_date")] DateOnly DueDate,
    [property: JsonPropertyName("manual_amount")] bool ManualAmount,
    [property: JsonPropertyName("item_net")] bool ItemNet,
    [property: JsonPropertyName("line_items")] IReadOnlyList<BillLineItem> LineItems,
    [property: JsonPropertyName("discounts")] IReadOnlyList<BillDiscount> Discounts,
    [property: JsonPropertyName("attachment_ids")] IReadOnlyList<Guid> AttachmentIds,
    [property: JsonPropertyName("vendor_ref")] string? VendorRef = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("amount_man")] decimal? AmountMan = null,
    [property: JsonPropertyName("amount_calc")] decimal? AmountCalc = null,
    [property: JsonPropertyName("exchange_rate")] decimal? ExchangeRate = null,
    [property: JsonPropertyName("base_currency_amount")] decimal? BaseCurrencyAmount = null,
    [property: JsonPropertyName("purchase_order_id")] int? PurchaseOrderId = null,
    [property: JsonPropertyName("qr_bill_information")] string? QrBillInformation = null,
    [property: JsonPropertyName("payment")] BillPayment? Payment = null
);
