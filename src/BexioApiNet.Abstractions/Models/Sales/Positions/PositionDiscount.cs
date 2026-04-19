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

namespace BexioApiNet.Abstractions.Models.Sales.Positions;

/// <summary>
///     Document-level discount position applied after the subtotal, either as a fixed amount or
///     a percentage of the running total.
///     Corresponds to the Bexio <c>KbPositionDiscount</c> / <c>PositionDiscountExtended</c> schema.
///     Unlike other variants, discount positions do not carry a <see cref="Position.ParentId" />.
///     <see href="https://docs.bexio.com/#tag/Discount-positions" />
/// </summary>
public sealed record PositionDiscount : Position
{
    /// <inheritdoc />
    public override string Type => PositionTypes.Discount;

    /// <summary>Discount label rendered on the document.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>
    ///     When <see langword="true" />, <see cref="Value" /> is interpreted as a percentage;
    ///     otherwise as a fixed amount in the document currency.
    /// </summary>
    [JsonPropertyName("is_percentual")]
    public bool? IsPercentual { get; init; }

    /// <summary>Discount amount as a formatted decimal string (max. 6 decimals).</summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    /// <summary>Effective discount total as a formatted decimal string (read-only).</summary>
    [JsonPropertyName("discount_total")]
    public string? DiscountTotal { get; init; }
}
