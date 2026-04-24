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

using BexioApiNet.Abstractions.Models.Expenses.Expenses.Enums;

namespace BexioApiNet.Abstractions.Models.Expenses.Expenses;

/// <summary>
/// Full expense entity returned by the single-item Bexio v4.0 <c>/expenses/expenses</c>
/// endpoints (GET by id, POST create, PUT update, POST actions, PUT bookings/{status}).
/// See <see cref="ExpenseListItem"/> for the condensed form returned by the list endpoint.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="Id">Unique expense identifier.</param>
/// <param name="DocumentNo">Unique expense document number. Auto-generated on creation.</param>
/// <param name="Status">Expense status.</param>
/// <param name="LastnameCompany">Supplier family name or company name.</param>
/// <param name="CreatedAt">Creation timestamp.</param>
/// <param name="SupplierId">Identifier of the supplier contact.</param>
/// <param name="ContactPartnerId">Identifier of the Bexio contact partner.</param>
/// <param name="ManualAmount">Whether <c>AmountMan</c> (true) or <c>AmountCalc</c> (false) is the authoritative expense amount.</param>
/// <param name="ExpenseDate">Expense issue date.</param>
/// <param name="DueDate">Due date for payment.</param>
/// <param name="Address">Address attached to the expense.</param>
/// <param name="LineItems">Line items on the expense.</param>
/// <param name="Discounts">Discounts applied to the expense.</param>
/// <param name="ItemNet">Whether line-item amounts are net (<see langword="true"/>) or gross (<see langword="false"/>).</param>
/// <param name="SplitIntoLineItems">Whether the expense has multiple line items or a single aggregate one.</param>
/// <param name="CurrencyCode">ISO currency code of the expense.</param>
/// <param name="BaseCurrencyCode">Base currency code taken from the Bexio account settings.</param>
/// <param name="Overdue">True when <see cref="DueDate"/> has passed. Not applicable to <c>DRAFT</c> or <c>PAID</c>.</param>
/// <param name="AttachmentIds">Bexio file identifiers attached to the expense.</param>
/// <param name="Title">Free-form expense title.</param>
/// <param name="FirstnameSuffix">Supplier first name or suffix.</param>
/// <param name="VendorRef">Vendor reference text.</param>
/// <param name="PendingAmount">Remaining amount to be paid.</param>
/// <param name="AmountMan">Manual amount. Considered authoritative when <see cref="ManualAmount"/> is <see langword="true"/>.</param>
/// <param name="AmountCalc">Calculated amount. Considered authoritative when <see cref="ManualAmount"/> is <see langword="false"/>.</param>
/// <param name="ExchangeRate">Exchange rate applied when <see cref="CurrencyCode"/> differs from <see cref="BaseCurrencyCode"/>.</param>
/// <param name="BaseCurrencyAmount">Expense amount expressed in the base currency.</param>
public sealed record Expense(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("document_no")] string DocumentNo,
    [property: JsonPropertyName("status")] ExpenseStatus Status,
    [property: JsonPropertyName("lastname_company")] string LastnameCompany,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("supplier_id")] int SupplierId,
    [property: JsonPropertyName("contact_partner_id")] int ContactPartnerId,
    [property: JsonPropertyName("manual_amount")] bool ManualAmount,
    [property: JsonPropertyName("expense_date")] DateOnly ExpenseDate,
    [property: JsonPropertyName("due_date")] DateOnly DueDate,
    [property: JsonPropertyName("address")] ExpenseAddress Address,
    [property: JsonPropertyName("line_items")] IReadOnlyList<ExpenseLineItem> LineItems,
    [property: JsonPropertyName("discounts")] IReadOnlyList<ExpenseDiscount> Discounts,
    [property: JsonPropertyName("item_net")] bool ItemNet,
    [property: JsonPropertyName("split_into_line_items")] bool SplitIntoLineItems,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("base_currency_code")] string BaseCurrencyCode,
    [property: JsonPropertyName("overdue")] bool Overdue,
    [property: JsonPropertyName("attachment_ids")] IReadOnlyList<Guid> AttachmentIds,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("firstname_suffix")] string? FirstnameSuffix = null,
    [property: JsonPropertyName("vendor_ref")] string? VendorRef = null,
    [property: JsonPropertyName("pending_amount")] decimal? PendingAmount = null,
    [property: JsonPropertyName("amount_man")] decimal? AmountMan = null,
    [property: JsonPropertyName("amount_calc")] decimal? AmountCalc = null,
    [property: JsonPropertyName("exchange_rate")] decimal? ExchangeRate = null,
    [property: JsonPropertyName("base_currency_amount")] decimal? BaseCurrencyAmount = null
);
