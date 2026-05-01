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

using System.Globalization;
using System.Text.Json;

namespace BexioApiNet.Abstractions.Json;

/// <summary>
/// <see cref="JsonConverter{T}" /> handling Bexio's space-separated <c>date-time</c> format
/// (<c>yyyy-MM-dd HH:mm:ss</c>) used by some v2 endpoints (e.g. note <c>event_start</c>).
/// On read, ISO 8601 values produced by other endpoints are accepted as a fallback so the
/// converter is safe to attach to fields whose format may shift between Bexio API versions.
/// On write, the converter always emits the Bexio space-separated format because that is
/// what the v2 endpoints expect on create / update payloads.
/// </summary>
public sealed class BexioDateTimeJsonConverter : JsonConverter<DateTime>
{
    private const string BexioDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    /// <inheritdoc />
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();

        if (raw is null)
            throw new JsonException("Expected a non-null date-time string.");

        if (DateTime.TryParseExact(raw, BexioDateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var bexio))
            return bexio;

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var iso))
            return iso;

        throw new JsonException($"Unable to parse '{raw}' as a Bexio date-time.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(BexioDateTimeFormat, CultureInfo.InvariantCulture));
    }
}
