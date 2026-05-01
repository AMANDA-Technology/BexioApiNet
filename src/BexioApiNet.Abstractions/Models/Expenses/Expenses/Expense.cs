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
/// Full expense entity returned by the single-item Bexio v4.0 <c>/expenses</c>
/// endpoints (GET by id, POST create, PUT update, POST actions, PUT bookings/{status}).
/// See <see cref="ExpenseListItem"/> for the condensed form returned by the list endpoint.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="Id">Unique expense identifier.</param>
/// <param name="Status">Expense status (<c>DRAFT</c> or <c>DONE</c>).</param>
/// <param name="PaidOn">Date the expense was paid on.</param>
/// <param name="CurrencyCode">ISO currency code of the expense (max length 20).</param>
/// <param name="Amount">Expense amount. Up to 17 digits, max 2 decimals.</param>
/// <param name="CreatedAt">Creation timestamp (date-time).</param>
/// <param name="BaseCurrencyCode">Base currency code taken from Bexio account settings (max length 20).</param>
/// <param name="AttachmentIds">Bexio file identifiers attached to the expense.</param>
/// <param name="DocumentNo">Unique expense document number. Auto-generated on creation.</param>
/// <param name="Title">Free-form expense title (max length 80).</param>
/// <param name="FirstnameSuffix">Supplier first name or suffix (max length 80).</param>
/// <param name="LastnameCompany">Supplier family name or company name (max length 80).</param>
/// <param name="SupplierId">Identifier of the supplier contact.</param>
/// <param name="BankAccountId">Bank account identifier used to settle the expense.</param>
/// <param name="BookingAccountId">Booking account identifier.</param>
/// <param name="ExchangeRate">Exchange rate applied when <see cref="CurrencyCode"/> differs from <see cref="BaseCurrencyCode"/>.</param>
/// <param name="TaxMan">Manual tax amount (deprecated outside the create response).</param>
/// <param name="TaxCalc">Server-calculated tax amount based on <see cref="Amount"/> and <see cref="TaxId"/>.</param>
/// <param name="TaxId">Tax identifier applied to the expense.</param>
/// <param name="BaseCurrencyAmount">Expense amount expressed in the base currency.</param>
/// <param name="TransactionId">Reconciled transaction identifier (set when the expense is reconciled).</param>
/// <param name="InvoiceId">Linked invoice identifier (set when this expense is linked to an invoice).</param>
/// <param name="ProjectId">Linked project identifier (set when this expense is linked to a project).</param>
/// <param name="Address">Address attached to the expense.</param>
public sealed record Expense(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("status")] ExpenseStatus Status,
    [property: JsonPropertyName("paid_on")] DateOnly PaidOn,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("base_currency_code")] string BaseCurrencyCode,
    [property: JsonPropertyName("attachment_ids")] IReadOnlyList<Guid> AttachmentIds,
    [property: JsonPropertyName("document_no")] string? DocumentNo = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("firstname_suffix")] string? FirstnameSuffix = null,
    [property: JsonPropertyName("lastname_company")] string? LastnameCompany = null,
    [property: JsonPropertyName("supplier_id")] int? SupplierId = null,
    [property: JsonPropertyName("bank_account_id")] int? BankAccountId = null,
    [property: JsonPropertyName("booking_account_id")] int? BookingAccountId = null,
    [property: JsonPropertyName("exchange_rate")] decimal? ExchangeRate = null,
    [property: JsonPropertyName("tax_man")] decimal? TaxMan = null,
    [property: JsonPropertyName("tax_calc")] decimal? TaxCalc = null,
    [property: JsonPropertyName("tax_id")] int? TaxId = null,
    [property: JsonPropertyName("base_currency_amount")] decimal? BaseCurrencyAmount = null,
    [property: JsonPropertyName("transaction_id")] Guid? TransactionId = null,
    [property: JsonPropertyName("invoice_id")] Guid? InvoiceId = null,
    [property: JsonPropertyName("project_id")] Guid? ProjectId = null,
    [property: JsonPropertyName("address")] ExpenseAddress? Address = null
);
