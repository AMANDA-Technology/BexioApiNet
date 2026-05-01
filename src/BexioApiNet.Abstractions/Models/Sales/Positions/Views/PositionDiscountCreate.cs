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
///     Create/update view for a discount position — body of
///     <c>POST /2.0/{kb_document_type}/{document_id}/kb_position_discount</c> (create) and
///     <c>POST /2.0/{kb_document_type}/{document_id}/kb_position_discount/{position_id}</c> (update).
///     Read-only fields returned by Bexio (<c>id</c>, <c>discount_total</c>) and the
///     <c>type</c> discriminator are intentionally omitted from the request shape — the
///     OpenAPI spec defines the request body as exactly <c>text</c>, <c>is_percentual</c>
///     and <c>value</c>.
///     <see href="https://docs.bexio.com/#tag/Discount-positions" />
/// </summary>
/// <param name="Text">Discount label rendered on the document.</param>
/// <param name="IsPercentual">
///     When <see langword="true" />, <see cref="Value" /> is interpreted as a percentage;
///     otherwise as a fixed amount in the document currency.
/// </param>
/// <param name="Value">Discount amount as a formatted decimal string (max. 6 decimals).</param>
public sealed record PositionDiscountCreate(
    [property: JsonPropertyName("text")] string? Text = null,
    [property: JsonPropertyName("is_percentual")]
    bool? IsPercentual = null,
    [property: JsonPropertyName("value")] string? Value = null
);
