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

using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Enums;

namespace BexioApiNet.Abstractions.Models.Accounting.CalendarYears;

/// <summary>
/// Calendar year returned by the Bexio accounting endpoint. <see href="https://docs.bexio.com/#tag/Calendar-Years/operation/ListCalendarYears">List Calendar Years</see>
/// </summary>
/// <param name="Id"></param>
/// <param name="Start"></param>
/// <param name="End"></param>
/// <param name="IsVatSubject"></param>
/// <param name="IsAnnualReporting"></param>
/// <param name="CreatedAt"></param>
/// <param name="UpdatedAt"></param>
/// <param name="VatAccountingMethod"></param>
/// <param name="VatAccountingType"></param>
public sealed record CalendarYear(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("start")] DateOnly Start,
    [property: JsonPropertyName("end")] DateOnly End,
    [property: JsonPropertyName("is_vat_subject")] bool IsVatSubject,
    [property: JsonPropertyName("is_annual_reporting")] bool IsAnnualReporting,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("updated_at")] DateTime UpdatedAt,
    [property: JsonPropertyName("vat_accounting_method")] VatAccountingMethod VatAccountingMethod,
    [property: JsonPropertyName("vat_accounting_type")] VatAccountingType VatAccountingType
);
