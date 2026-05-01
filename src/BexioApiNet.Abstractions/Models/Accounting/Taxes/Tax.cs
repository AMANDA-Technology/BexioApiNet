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
/// <param name="Id">The id of the tax.</param>
/// <param name="Uuid">The uuid of the tax.</param>
/// <param name="Name">Internal name of the tax. Maximum 80 characters.</param>
/// <param name="Code">Tax code (e.g. <c>UN77</c>). Maximum 80 characters.</param>
/// <param name="Digit">Tax statement digit (e.g. <c>200</c>, <c>205.301</c>).</param>
/// <param name="Type">Tax type (e.g. <c>sales_tax</c>, <c>pre_tax_material</c>).</param>
/// <param name="AccountId">The id of the account associated with the tax.</param>
/// <param name="TaxSettlementType">Tax settlement type. Maximum 80 characters.</param>
/// <param name="Value">Tax percentage (e.g. 7.7).</param>
/// <param name="NetTaxValue">Net tax value as a string. Used when <paramref name="Type"/> is <c>net_tax</c>; otherwise <see langword="null"/>.</param>
/// <param name="StartYear">Year from which the tax becomes effective. <see langword="null"/> means no lower bound.</param>
/// <param name="EndYear">Last year for which the tax is effective. <see langword="null"/> means no upper bound.</param>
/// <param name="IsActive">Indicates whether the tax is currently active.</param>
/// <param name="DisplayName">Localized display name. Maximum 255 characters.</param>
/// <param name="StartMonth">Month within <paramref name="StartYear"/> from which the tax becomes effective.</param>
/// <param name="EndMonth">Month within <paramref name="EndYear"/> through which the tax is effective.</param>
public sealed record Tax(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("uuid")] string? Uuid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("digit")] string? Digit,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("account_id")] int? AccountId,
    [property: JsonPropertyName("tax_settlement_type")] string? TaxSettlementType,
    [property: JsonPropertyName("value")] decimal Value,
    [property: JsonPropertyName("net_tax_value")] string? NetTaxValue,
    [property: JsonPropertyName("start_year")] int? StartYear,
    [property: JsonPropertyName("end_year")] int? EndYear,
    [property: JsonPropertyName("is_active")] bool IsActive,
    [property: JsonPropertyName("display_name")] string DisplayName,
    [property: JsonPropertyName("start_month")] int? StartMonth,
    [property: JsonPropertyName("end_month")] int? EndMonth
);
