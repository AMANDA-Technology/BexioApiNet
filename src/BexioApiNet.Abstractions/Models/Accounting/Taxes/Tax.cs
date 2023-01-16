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

namespace BexioApiNet.Abstractions.Models.Accounting.Taxes;

/// <summary>
/// Tax object. <see href="https://docs.bexio.com/#tag/Taxes"/>
/// </summary>
/// <param name="Id"></param>
/// <param name="Uuid"></param>
/// <param name="Name"></param>
/// <param name="Code"></param>
/// <param name="Digit"></param>
/// <param name="Type"></param>
/// <param name="AccountId"></param>
/// <param name="TaxSettlementType"></param>
/// <param name="Value"></param>
/// <param name="NetTaxValue"></param>
/// <param name="StartYear"></param>
/// <param name="EndYear"></param>
/// <param name="IsActive"></param>
/// <param name="DisplayName"></param>
/// <param name="StartMonth"></param>
/// <param name="EndMonth"></param>
public sealed record Tax(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("uuid")] string Uuid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("digit")] string Digit,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("account_id")] int? AccountId,
    [property: JsonPropertyName("tax_settlement_type")] string TaxSettlementType,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("net_tax_value")] int? NetTaxValue,
    [property: JsonPropertyName("start_year")] int? StartYear,
    [property: JsonPropertyName("end_year")] int? EndYear,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("start_month")] int? StartMonth,
    [property: JsonPropertyName("end_month")] int? EndMonth
);
