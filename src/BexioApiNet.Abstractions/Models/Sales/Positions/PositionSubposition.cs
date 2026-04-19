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
///     Container / heading position used to group other positions underneath a shared label and
///     a running subtotal. Other positions point to it via their <see cref="Position.ParentId" />.
///     Corresponds to the Bexio <c>KbPositionSubposition</c> / <c>PositionSubpositionExtended</c>
///     schema. Sub-positions are only valid on quotes, orders and invoices — not deliveries.
///     <see href="https://docs.bexio.com/#tag/Sub-positions" />
/// </summary>
public sealed record PositionSubposition : Position
{
    /// <inheritdoc />
    public override string Type => PositionTypes.Subposition;

    /// <summary>Heading text rendered above the grouped positions.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>Rendered position number on the document (read-only).</summary>
    [JsonPropertyName("pos")]
    public string? Pos { get; init; }

    /// <summary>Internal 1-based position ordering (read-only).</summary>
    [JsonPropertyName("internal_pos")]
    public int? InternalPos { get; init; }

    /// <summary>When <see langword="true" />, the group's running position number is rendered.</summary>
    [JsonPropertyName("show_pos_nr")]
    public bool? ShowPosNr { get; init; }

    /// <summary>When <see langword="true" />, the group is marked as optional on quotes/orders (read-only).</summary>
    [JsonPropertyName("is_optional")]
    public bool? IsOptional { get; init; }

    /// <summary>Aggregated total of all child positions as a formatted decimal string (read-only).</summary>
    [JsonPropertyName("total_sum")]
    public string? TotalSum { get; init; }

    /// <summary>When <see langword="true" />, individual child prices are rendered (read-only).</summary>
    [JsonPropertyName("show_pos_prices")]
    public bool? ShowPosPrices { get; init; }
}
