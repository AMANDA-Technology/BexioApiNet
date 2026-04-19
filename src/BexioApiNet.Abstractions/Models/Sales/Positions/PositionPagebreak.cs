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
///     Page-break position that forces a hard page break at its location on the printed document.
///     Corresponds to the Bexio <c>KbPositionPagebreak</c> / <c>PositionPagebreakExtended</c>
///     schema.
///     <see href="https://docs.bexio.com/#tag/Pagebreak-positions" />
/// </summary>
public sealed record PositionPagebreak : Position
{
    /// <inheritdoc />
    public override string Type => PositionTypes.Pagebreak;

    /// <summary>Internal 1-based position ordering (read-only).</summary>
    [JsonPropertyName("internal_pos")]
    public int? InternalPos { get; init; }

    /// <summary>When <see langword="true" />, the position is marked as optional on quotes/orders (read-only).</summary>
    [JsonPropertyName("is_optional")]
    public bool? IsOptional { get; init; }

    /// <summary>
    ///     Must be <see langword="true" /> on create to actually insert a pagebreak (write-only flag
    ///     echoed back by the spec).
    /// </summary>
    [JsonPropertyName("pagebreak")]
    public bool? Pagebreak { get; init; }
}
