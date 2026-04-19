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
/// Free-text position used to inject a paragraph of text into a document without any
/// pricing or quantity. Corresponds to the Bexio <c>KbPositionText</c> /
/// <c>PositionTextExtended</c> schema.
/// <see href="https://docs.bexio.com/#tag/Text-positions" />
/// </summary>
public sealed record PositionText : Position
{
    /// <inheritdoc />
    public override string Type => PositionTypes.Text;

    /// <summary>Free text rendered on the printed document.</summary>
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    /// <summary>When <see langword="true" />, the automatic position number is rendered next to the text.</summary>
    [JsonPropertyName("show_pos_nr")]
    public bool? ShowPosNr { get; init; }

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
