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
/// Manual entry as returned by the Bexio accounting manual entry endpoints.
/// <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/ListManualEntries"/>
/// </summary>
/// <param name="Id">The id of the manual entry.</param>
/// <param name="Type">Type of the manual entry (manual_single_entry, manual_compound_entry, manual_group_entry).</param>
/// <param name="Date">Date of the manual entry.</param>
/// <param name="ReferenceNr">Reference number of the manual entry.</param>
/// <param name="CreatedByUserId">The id of the user who created the manual entry.</param>
/// <param name="EditedByUserId">The id of the user who last edited the manual entry.</param>
/// <param name="Entries">List of accounting entry lines that compose this manual entry.</param>
/// <param name="IsLocked">Indicates whether the manual entry is locked.</param>
/// <param name="LockedInfo">Reason why the manual entry is locked (closed_business_year, closed_tax_period, is_generated, Banking_transaction, locked_business_year).</param>
public sealed record ManualEntry(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("date")] DateOnly Date,
    [property: JsonPropertyName("reference_nr")] string? ReferenceNr,
    [property: JsonPropertyName("created_by_user_id")] int? CreatedByUserId,
    [property: JsonPropertyName("edited_by_user_id")] int? EditedByUserId,
    [property: JsonPropertyName("entries")] IReadOnlyList<ManualEntryEntry> Entries,
    [property: JsonPropertyName("is_locked")] bool? IsLocked,
    [property: JsonPropertyName("locked_info")] string? LockedInfo
);
