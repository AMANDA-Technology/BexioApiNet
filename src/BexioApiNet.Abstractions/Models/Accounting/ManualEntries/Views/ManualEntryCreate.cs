﻿/*
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

using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Enums;

namespace BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Views;

/// <summary>
/// Create view for a manual entry. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/CreateManualEntry">Create Manual Entry</see>
/// </summary>
/// <param name="DebitAccountId"></param>
/// <param name="CreditAccountId"></param>
/// <param name="TaxId"></param>
/// <param name="TaxAccountId"></param>
/// <param name="Description"></param>
/// <param name="Amount"></param>
/// <param name="CurrencyId"></param>
/// <param name="CurrencyFactor"></param>
public sealed record ManualEntryCreate(
    [property: JsonPropertyName("debit_account_id")] int? DebitAccountId,
    [property: JsonPropertyName("credit_account_id")] int? CreditAccountId,
    [property: JsonPropertyName("tax_id")] int? TaxId,
    [property: JsonPropertyName("tax_account_id")] int? TaxAccountId,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("amount")] decimal? Amount,
    [property: JsonPropertyName("currency_id")] int? CurrencyId,
    [property: JsonPropertyName("currency_factor")] decimal? CurrencyFactor
);

/// <summary>
/// Create view for a manual entry. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/CreateManualEntry">Create Manual Entry</see>
/// </summary>
/// <param name="Type"></param>
/// <param name="Date"></param>
/// <param name="ReferenceNr"></param>
/// <param name="Entries"></param>
public sealed record ManualEntryEntryCreate(
    [property: JsonPropertyName("type")] ManualEntryType Type,
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("reference_nr")] string ReferenceNr,
    [property: JsonPropertyName("entries")] IReadOnlyList<ManualEntryCreate> Entries
);
