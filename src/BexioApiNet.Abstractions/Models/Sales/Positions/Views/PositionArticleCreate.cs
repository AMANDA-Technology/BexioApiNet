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

namespace BexioApiNet.Abstractions.Models.Sales.Positions.Views;

/// <summary>
///     Create/update view for an article (item) position — body of
///     <c>POST /2.0/{kb_document_type}/{document_id}/kb_position_article</c> (create) and
///     <c>POST /2.0/{kb_document_type}/{document_id}/kb_position_article/{position_id}</c> (update).
///     Read-only fields returned by Bexio (<c>amount_reserved</c>, <c>amount_open</c>,
///     <c>amount_completed</c>, <c>unit_name</c>, <c>tax_value</c>, <c>position_total</c>,
///     <c>pos</c>, <c>internal_pos</c>) are intentionally omitted.
///     <see href="https://docs.bexio.com/#tag/Article-positions" />
/// </summary>
/// <param name="Amount">Quantity as a formatted decimal string (max. 6 decimals).</param>
/// <param name="UnitId">References a unit object.</param>
/// <param name="AccountId">References an account object.</param>
/// <param name="TaxId">References a tax object. Only active sales taxes are valid.</param>
/// <param name="Text">Position text / description shown on the printed document.</param>
/// <param name="UnitPrice">Price of one unit as a formatted decimal string (max. 6 decimals).</param>
/// <param name="DiscountInPercent">Per-position discount as a formatted decimal string (max. 6 decimals).</param>
/// <param name="IsOptional">When <see langword="true" />, the position is marked as optional on quotes/orders.</param>
/// <param name="ArticleId">References an item object in the Bexio article catalogue.</param>
/// <param name="ParentId">Optional id of a parent <c>kb_position_subposition</c> used to group positions.</param>
public sealed record PositionArticleCreate(
    [property: JsonPropertyName("amount")] string? Amount = null,
    [property: JsonPropertyName("unit_id")]
    int? UnitId = null,
    [property: JsonPropertyName("account_id")]
    int? AccountId = null,
    [property: JsonPropertyName("tax_id")] int? TaxId = null,
    [property: JsonPropertyName("text")] string? Text = null,
    [property: JsonPropertyName("unit_price")]
    string? UnitPrice = null,
    [property: JsonPropertyName("discount_in_percent")]
    string? DiscountInPercent = null,
    [property: JsonPropertyName("is_optional")]
    bool? IsOptional = null,
    [property: JsonPropertyName("article_id")]
    int? ArticleId = null,
    [property: JsonPropertyName("parent_id")]
    int? ParentId = null
);