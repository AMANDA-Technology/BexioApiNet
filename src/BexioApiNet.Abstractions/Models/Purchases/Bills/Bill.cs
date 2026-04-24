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

using BexioApiNet.Abstractions.Models.Purchases.Bills.Enums;

namespace BexioApiNet.Abstractions.Models.Purchases.Bills;

/// <summary>
/// Full bill entity returned by the single-item Bexio v4.0 <c>/purchase/bills</c>
/// endpoints (GET by id, POST create, PUT update, POST actions, PUT bookings/{status}).
/// See <see cref="BillListItem"/> for the condensed form returned by the list endpoint.
/// <see href="https://docs.bexio.com/#tag/Bills">Bills</see>
/// </summary>
/// <param name="Id">Unique bill identifier.</param>
/// <param name="DocumentNo">Unique bill document number. Auto-generated on creation.</param>
/// <param name="Status">Bill status.</param>
/// <param name="LastnameCompany">Supplier family name or company name.</param>
/// <param name="CreatedAt">Creation timestamp.</param>
/// <param name="SupplierId">Identifier of the supplier contact.</param>
/// <param name="ContactPartnerId">Identifier of the Bexio contact partner.</param>
/// <param name="ManualAmount">Whether <c>AmountMan</c> (true) or <c>AmountCalc</c> (false) is the authoritative bill amount.</param>
/// <param name="BillDate">Bill issue date.</param>
/// <param name="DueDate">Due date for payment.</param>
/// <param name="Address">Address attached to the bill.</param>
/// <param name="LineItems">Line items on the bill.</param>
/// <param name="Discounts">Discounts applied to the bill.</param>
/// <param name="ItemNet">Whether line-item amounts are net (<see langword="true"/>) or gross (<see langword="false"/>).</param>
/// <param name="SplitIntoLineItems">Whether the bill has multiple line items or a single aggregate one.</param>
/// <param name="BaseCurrencyCode">Base currency code taken from the Bexio account settings.</param>
/// <param name="Overdue">True when <see cref="DueDate"/> has passed. Not applicable to <c>DRAFT</c> or <c>PAID</c>.</param>
/// <param name="AttachmentIds">Bexio file identifiers attached to the bill.</param>
/// <param name="Title">Free-form bill title.</param>
/// <param name="FirstnameSuffix">Supplier first name or suffix.</param>
/// <param name="VendorRef">Vendor reference text.</param>
/// <param name="PendingAmount">Remaining amount to be paid.</param>
/// <param name="AmountMan">Manual amount. Considered authoritative when <see cref="ManualAmount"/> is <see langword="true"/>.</param>
/// <param name="AmountCalc">Calculated amount. Considered authoritative when <see cref="ManualAmount"/> is <see langword="false"/>.</param>
/// <param name="CurrencyCode">ISO currency code of the bill.</param>
/// <param name="ExchangeRate">Exchange rate applied when <see cref="CurrencyCode"/> differs from <see cref="BaseCurrencyCode"/>.</param>
/// <param name="PurchaseOrderId">Optional purchase-order identifier this bill originates from.</param>
/// <param name="BaseCurrencyAmount">Bill amount expressed in the base currency.</param>
/// <param name="QrBillInformation">QR bill payload string when the bill was captured from a QR invoice.</param>
/// <param name="Payment">Optional payment block attached to the bill.</param>
/// <param name="AverageExchangeRateEnabled">Whether the average exchange rate feature is enabled for this bill.</param>
public sealed record Bill(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("document_no")] string DocumentNo,
    [property: JsonPropertyName("status")] BillStatus Status,
    [property: JsonPropertyName("lastname_company")] string LastnameCompany,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("supplier_id")] int SupplierId,
    [property: JsonPropertyName("contact_partner_id")] int ContactPartnerId,
    [property: JsonPropertyName("manual_amount")] bool ManualAmount,
    [property: JsonPropertyName("bill_date")] DateOnly BillDate,
    [property: JsonPropertyName("due_date")] DateOnly DueDate,
    [property: JsonPropertyName("address")] BillAddress Address,
    [property: JsonPropertyName("line_items")] IReadOnlyList<BillLineItem> LineItems,
    [property: JsonPropertyName("discounts")] IReadOnlyList<BillDiscount> Discounts,
    [property: JsonPropertyName("item_net")] bool ItemNet,
    [property: JsonPropertyName("split_into_line_items")] bool SplitIntoLineItems,
    [property: JsonPropertyName("base_currency_code")] string BaseCurrencyCode,
    [property: JsonPropertyName("overdue")] bool Overdue,
    [property: JsonPropertyName("attachment_ids")] IReadOnlyList<Guid> AttachmentIds,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("firstname_suffix")] string? FirstnameSuffix = null,
    [property: JsonPropertyName("vendor_ref")] string? VendorRef = null,
    [property: JsonPropertyName("pending_amount")] decimal? PendingAmount = null,
    [property: JsonPropertyName("amount_man")] decimal? AmountMan = null,
    [property: JsonPropertyName("amount_calc")] decimal? AmountCalc = null,
    [property: JsonPropertyName("currency_code")] string? CurrencyCode = null,
    [property: JsonPropertyName("exchange_rate")] decimal? ExchangeRate = null,
    [property: JsonPropertyName("purchase_order_id")] int? PurchaseOrderId = null,
    [property: JsonPropertyName("base_currency_amount")] decimal? BaseCurrencyAmount = null,
    [property: JsonPropertyName("qr_bill_information")] string? QrBillInformation = null,
    [property: JsonPropertyName("payment")] BillPayment? Payment = null,
    [property: JsonPropertyName("average_exchange_rate_enabled")] bool? AverageExchangeRateEnabled = null
);
