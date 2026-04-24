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

namespace BexioApiNet.Abstractions.Models.Expenses.Expenses;

/// <summary>
/// A line item on an expense. Shared by the create/update payloads and the responses —
/// on creation <c>Id</c> and <c>TaxCalc</c> are absent and set by the server; on
/// update callers must echo the existing <c>Id</c> or omit it to create a new item.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="Position">Zero-based position of the line item within the expense.</param>
/// <param name="Amount">Line item amount (net or gross per <c>item_net</c>). Max 17 digits, 2 decimals.</param>
/// <param name="Id">Line item identifier. Server-generated on create; echoed on update to preserve, or <see langword="null"/> to create a new item.</param>
/// <param name="Title">Optional line item title.</param>
/// <param name="TaxId">Optional tax identifier applied to this line item.</param>
/// <param name="TaxCalc">Server-calculated tax amount based on <c>Amount</c> and <c>TaxId</c>. Read-only.</param>
/// <param name="BookingAccountId">Booking account identifier. Required when the expense transitions to <c>BOOKED</c>.</param>
public sealed record ExpenseLineItem(
    [property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("id")] Guid? Id = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("tax_id")] int? TaxId = null,
    [property: JsonPropertyName("tax_calc")] decimal? TaxCalc = null,
    [property: JsonPropertyName("booking_account_id")] int? BookingAccountId = null
);
