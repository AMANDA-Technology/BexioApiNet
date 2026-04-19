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

using BexioApiNet.Abstractions.Models.Accounting.VatPeriods.Enums;

namespace BexioApiNet.Abstractions.Models.Accounting.VatPeriods;

/// <summary>
/// Vat period object. <see href="https://docs.bexio.com/#tag/Vat-Periods"/>
/// </summary>
/// <param name="Id">The id of the vat period.</param>
/// <param name="Start">Start date of the vat period.</param>
/// <param name="End">End date of the vat period.</param>
/// <param name="Type">Duration of the vat period (quarter, semester, annual).</param>
/// <param name="Status">Status of the vat period (open, closed, closed_with_message).</param>
/// <param name="ClosedAt">Closed date of the vat period; <see langword="null"/> while the period is still open.</param>
public sealed record VatPeriod(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("start")] DateOnly Start,
    [property: JsonPropertyName("end")] DateOnly End,
    [property: JsonPropertyName("type")] VatPeriodType Type,
    [property: JsonPropertyName("status")] VatPeriodStatus Status,
    [property: JsonPropertyName("closed_at")] DateOnly? ClosedAt
);
