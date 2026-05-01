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

namespace BexioApiNet.Abstractions.Models.Accounting.ManualEntries;

/// <summary>
/// Manual entry line as returned inside a <see cref="ManualEntry"/>.
/// <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/ListManualEntries"/>
/// </summary>
/// <param name="Id">The id of the entry line.</param>
/// <param name="Date">Date of the entry line.</param>
/// <param name="DebitAccountId">The id of the debit account.</param>
/// <param name="CreditAccountId">The id of the credit account.</param>
/// <param name="TaxId">The id of the tax applied to the entry.</param>
/// <param name="TaxAccountId">The id of the tax account; must equal the debit or credit account id.</param>
/// <param name="Description">A description for the entry. Maximum 255 characters.</param>
/// <param name="Amount">The amount of the entry.</param>
/// <param name="CurrencyId">The id of the referenced currency.</param>
/// <param name="BaseCurrencyId">The id of the currency used in the general ledger.</param>
/// <param name="CurrencyFactor">The exchange factor between <paramref name="CurrencyId"/> and <paramref name="BaseCurrencyId"/>.</param>
/// <param name="BaseCurrencyAmount">The total amount of the entry in the currency of the general ledger.</param>
/// <param name="CreatedByUserId">The id of the user who created the entry line.</param>
/// <param name="EditedByUserId">The id of the user who last edited the entry line.</param>
public sealed record ManualEntryEntry(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("date")] DateOnly? Date,
    [property: JsonPropertyName("debit_account_id")] int? DebitAccountId,
    [property: JsonPropertyName("credit_account_id")] int? CreditAccountId,
    [property: JsonPropertyName("tax_id")] int? TaxId,
    [property: JsonPropertyName("tax_account_id")] int? TaxAccountId,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("amount")] decimal? Amount,
    [property: JsonPropertyName("currency_id")] int? CurrencyId,
    [property: JsonPropertyName("base_currency_id")] int? BaseCurrencyId,
    [property: JsonPropertyName("currency_factor")] decimal? CurrencyFactor,
    [property: JsonPropertyName("base_currency_amount")] decimal? BaseCurrencyAmount,
    [property: JsonPropertyName("created_by_user_id")] int? CreatedByUserId,
    [property: JsonPropertyName("edited_by_user_id")] int? EditedByUserId
);
