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

namespace BexioApiNet.Abstractions.Models.Expenses.Expenses.Views;

/// <summary>
/// Update view for <c>PUT /4.0/expenses/{id}</c>. When updating an expense in <c>DONE</c>
/// status only <c>attachment_ids</c> is actually persisted server-side — the other fields
/// must still be supplied but are ignored.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="PaidOn">Date the expense was paid on.</param>
/// <param name="CurrencyCode">ISO currency code of the expense (max length 20).</param>
/// <param name="Amount">Expense amount. Up to 17 digits, max 2 decimals. Must be ≥ 0.</param>
/// <param name="AttachmentIds">Bexio file identifiers attached to the expense.</param>
/// <param name="SupplierId">Identifier of the supplier contact.</param>
/// <param name="DocumentNo">Override the auto-generated expense document number (max length 255).</param>
/// <param name="Title">Optional free-form expense title (max length 80).</param>
/// <param name="BankAccountId">Bank account identifier.</param>
/// <param name="BookingAccountId">Booking account identifier.</param>
/// <param name="TaxId">Tax identifier.</param>
/// <param name="ExchangeRate">Exchange rate. Required when <see cref="CurrencyCode"/> differs from the base currency.</param>
/// <param name="BaseCurrencyAmount">Expense amount expressed in the base currency.</param>
/// <param name="Address">Optional address block describing the supplier.</param>
public sealed record ExpenseUpdate(
    [property: JsonPropertyName("paid_on")] DateOnly PaidOn,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("attachment_ids")] IReadOnlyList<Guid> AttachmentIds,
    [property: JsonPropertyName("supplier_id")] int? SupplierId = null,
    [property: JsonPropertyName("document_no")] string? DocumentNo = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("bank_account_id")] int? BankAccountId = null,
    [property: JsonPropertyName("booking_account_id")] int? BookingAccountId = null,
    [property: JsonPropertyName("tax_id")] int? TaxId = null,
    [property: JsonPropertyName("exchange_rate")] decimal? ExchangeRate = null,
    [property: JsonPropertyName("base_currency_amount")] decimal? BaseCurrencyAmount = null,
    [property: JsonPropertyName("address")] ExpenseAddress? Address = null
);
