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
///     Free-form / custom line item not backed by an article.
///     Corresponds to the Bexio <c>KbPositionCustom</c> / <c>PositionCustomExtended</c> schema.
///     <see href="https://docs.bexio.com/#tag/Custom-positions" />
/// </summary>
public sealed record PositionCustom : Position
{
    /// <inheritdoc />
    public override string Type => PositionTypes.Custom;

    /// <summary>Quantity as a formatted decimal string (max. 6 decimals).</summary>
    [JsonPropertyName("amount")]
    public string? Amount { get; init; }

    /// <summary>Reserved quantity as a formatted decimal string (read-only).</summary>
    [JsonPropertyName("amount_reserved")]
    public string? AmountReserved { get; init; }

    /// <summary>Open quantity remaining to deliver/invoice as a formatted decimal string (read-only).</summary>
    [JsonPropertyName("amount_open")]
    public string? AmountOpen { get; init; }

    /// <summary>Completed quantity as a formatted decimal string (read-only).</summary>
    [JsonPropertyName("amount_completed")]
    public string? AmountCompleted { get; init; }

    /// <summary>References a unit object.</summary>
    [JsonPropertyName("unit_id")]
    public int? UnitId { get; init; }

    /// <summary>References an account object.</summary>
    [JsonPropertyName("account_id")]
    public int? AccountId { get; init; }

    /// <summary>Display name of the unit (read-only).</summary>
    [JsonPropertyName("unit_name")]
    public string? UnitName { get; init; }

    /// <summary>References a tax object. Only active sales taxes are valid on quotes/orders/invoices.</summary>
    [JsonPropertyName("tax_id")]
    public int? TaxId { get; init; }

    /// <summary>Tax percentage value as a formatted string (read-only).</summary>
    [JsonPropertyName("tax_value")]
    public string? TaxValue { get; init; }

    /// <summary>Position text / description shown on the printed document.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Price of one unit as a formatted decimal string (max. 6 decimals).</summary>
    [JsonPropertyName("unit_price")]
    public string? UnitPrice { get; init; }

    /// <summary>Per-position discount as a formatted decimal string (max. 6 decimals).</summary>
    [JsonPropertyName("discount_in_percent")]
    public string? DiscountInPercent { get; init; }

    /// <summary>Line total as a formatted decimal string (read-only).</summary>
    [JsonPropertyName("position_total")]
    public string? PositionTotal { get; init; }

    /// <summary>Rendered position number on the document (read-only).</summary>
    [JsonPropertyName("pos")]
    public string? Pos { get; init; }

    /// <summary>Internal 1-based position ordering (read-only).</summary>
    [JsonPropertyName("internal_pos")]
    public int? InternalPos { get; init; }

    /// <summary>When <see langword="true" />, the position is marked as optional on quotes/orders (read-only).</summary>
    [JsonPropertyName("is_optional")]
    public bool? IsOptional { get; init; }
}
