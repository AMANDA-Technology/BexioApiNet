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

namespace BexioApiNet.Abstractions.Models.MasterData.Languages;

/// <summary>
/// A Bexio language lookup entry. <see href="https://docs.bexio.com/#tag/Languages"/>
/// </summary>
public sealed record Language
{
    /// <summary>
    /// Unique language identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Display name of the language (e.g. <c>German</c>).
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Decimal point character used when formatting numbers in this language (e.g. <c>.</c>).
    /// </summary>
    [JsonPropertyName("decimal_point")]
    public string? DecimalPoint { get; init; }

    /// <summary>
    /// Thousands separator used when formatting numbers in this language (e.g. <c>'</c>).
    /// </summary>
    [JsonPropertyName("thousands_separator")]
    public string? ThousandsSeparator { get; init; }

    /// <summary>
    /// Numeric id of the date format: <c>1</c> → <c>DD.MM.YYYY</c>, <c>2</c> → <c>MM/DD/YYYY</c>.
    /// </summary>
    [JsonPropertyName("date_format_id")]
    public int? DateFormatId { get; init; }

    /// <summary>
    /// PHP-style date format pattern (e.g. <c>d.m.Y</c>).
    /// </summary>
    [JsonPropertyName("date_format")]
    public string? DateFormat { get; init; }

    /// <summary>
    /// ISO 639-1 two-letter language code (e.g. <c>de</c>).
    /// </summary>
    [JsonPropertyName("iso_639_1")]
    public required string Iso6391 { get; init; }
}
