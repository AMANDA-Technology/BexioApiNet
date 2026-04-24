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
/// Condensed bill entry returned by the list endpoint
/// <c>GET /4.0/purchase/bills</c>. See <see cref="Bill"/> for the full model returned
/// by the single-item endpoints.
/// <see href="https://docs.bexio.com/#tag/Bills">Bills</see>
/// </summary>
/// <param name="Id">Unique bill identifier.</param>
/// <param name="DocumentNo">Unique bill document number.</param>
/// <param name="Status">Bill status.</param>
/// <param name="LastnameCompany">Supplier family name or company name.</param>
/// <param name="Vendor">Joined <c>firstname_suffix</c> and <c>lastname_company</c> based on supplier type.</param>
/// <param name="CreatedAt">Creation timestamp.</param>
/// <param name="CurrencyCode">ISO currency code of the bill.</param>
/// <param name="BillDate">Bill date.</param>
/// <param name="DueDate">Due date.</param>
/// <param name="BookingAccountIds">Distinct booking account ids used across the bill's line items.</param>
/// <param name="Overdue">True when <see cref="DueDate"/> has passed; always <see langword="false"/> for <c>DRAFT</c> and <c>PAID</c>.</param>
/// <param name="AttachmentIds">Bexio file identifiers attached to the bill.</param>
/// <param name="Title">Bill title.</param>
/// <param name="FirstnameSuffix">Supplier first name or suffix.</param>
/// <param name="VendorRef">Vendor reference text.</param>
/// <param name="PendingAmount">Remaining amount to be paid.</param>
/// <param name="Net">Net value of the bill, calculated from line items and discounts.</param>
/// <param name="Gross">Gross value of the bill, calculated from line items and discounts.</param>
public sealed record BillListItem(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("document_no")] string DocumentNo,
    [property: JsonPropertyName("status")] BillStatus Status,
    [property: JsonPropertyName("lastname_company")] string LastnameCompany,
    [property: JsonPropertyName("vendor")] string Vendor,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("bill_date")] DateOnly BillDate,
    [property: JsonPropertyName("due_date")] DateOnly DueDate,
    [property: JsonPropertyName("booking_account_ids")] IReadOnlyList<int> BookingAccountIds,
    [property: JsonPropertyName("overdue")] bool Overdue,
    [property: JsonPropertyName("attachment_ids")] IReadOnlyList<Guid> AttachmentIds,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("firstname_suffix")] string? FirstnameSuffix = null,
    [property: JsonPropertyName("vendor_ref")] string? VendorRef = null,
    [property: JsonPropertyName("pending_amount")] decimal? PendingAmount = null,
    [property: JsonPropertyName("net")] decimal? Net = null,
    [property: JsonPropertyName("gross")] decimal? Gross = null
);
