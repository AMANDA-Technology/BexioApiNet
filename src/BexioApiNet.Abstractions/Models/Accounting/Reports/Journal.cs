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

namespace BexioApiNet.Abstractions.Models.Accounting.Reports;

/// <summary>
/// Journal entry as returned by the Bexio accounting journal endpoint. <see href="https://docs.bexio.com/#tag/Reports/operation/ListJournalEntries"/>
/// </summary>
/// <param name="Id">The id of the journal entry.</param>
/// <param name="RefId">Referenced id of the entry.</param>
/// <param name="RefUuid">Referenced uuid of the entry.</param>
/// <param name="RefClass">Referenced class of the entry.</param>
/// <param name="Date">Entry date for the entry.</param>
/// <param name="DebitAccountId">The id of the debit account.</param>
/// <param name="CreditAccountId">The id of the credit account.</param>
/// <param name="Description">A description for the entry.</param>
/// <param name="Amount">The total amount of the entry.</param>
/// <param name="CurrencyId">The id of the referenced currency.</param>
/// <param name="CurrencyFactor">The exchange factor of the currency_id and base_currency_id.</param>
/// <param name="BaseCurrencyId">The id of the currency used in the general ledger.</param>
/// <param name="BaseCurrencyAmount">The total amount of the entry in the currency of the general ledger.</param>
public sealed record Journal(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("ref_id")] int? RefId,
    [property: JsonPropertyName("ref_uuid")] string? RefUuid,
    [property: JsonPropertyName("ref_class")] string? RefClass,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("debit_account_id")] int? DebitAccountId,
    [property: JsonPropertyName("credit_account_id")] int? CreditAccountId,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency_id")] int? CurrencyId,
    [property: JsonPropertyName("currency_factor")] decimal? CurrencyFactor,
    [property: JsonPropertyName("base_currency_id")] int? BaseCurrencyId,
    [property: JsonPropertyName("base_currency_amount")] decimal BaseCurrencyAmount
);
