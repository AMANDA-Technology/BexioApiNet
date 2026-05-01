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

using System.Text.Json;
using System.Text.Json.Serialization;
using BexioApiNet.Abstractions.Json;

namespace BexioApiNet.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for <see cref="BexioDateTimeJsonConverter" />. The converter handles the
/// Bexio v2 <c>date-time</c> format <c>yyyy-MM-dd HH:mm:ss</c> (note the space, not the
/// ISO 8601 <c>T</c>) and falls back to ISO 8601 on read so it is safe to attach to fields
/// whose format may shift across endpoints.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class BexioDateTimeJsonConverterTests
{
    private sealed record Wrapper([property: JsonConverter(typeof(BexioDateTimeJsonConverter))] DateTime Value);

    /// <summary>
    /// A Bexio space-separated <c>date-time</c> deserializes into the expected
    /// <see cref="DateTime" /> value.
    /// </summary>
    [Test]
    public void Read_BexioFormat_ParsesDateTime()
    {
        const string json = "{\"Value\":\"2026-01-16 14:20:00\"}";

        var wrapper = JsonSerializer.Deserialize<Wrapper>(json);

        wrapper.ShouldNotBeNull();
        wrapper.Value.ShouldBe(new DateTime(2026, 1, 16, 14, 20, 0));
    }

    /// <summary>
    /// An ISO 8601 <c>date-time</c> still deserializes successfully (fallback path) so
    /// fields whose format varies across Bexio API versions remain robust.
    /// </summary>
    [Test]
    public void Read_IsoFormat_FallsBackToDefaultParser()
    {
        const string json = "{\"Value\":\"2026-01-16T14:20:00\"}";

        var wrapper = JsonSerializer.Deserialize<Wrapper>(json);

        wrapper.ShouldNotBeNull();
        wrapper.Value.ShouldBe(new DateTime(2026, 1, 16, 14, 20, 0));
    }

    /// <summary>
    /// A non-date-time string raises <see cref="JsonException" /> rather than silently
    /// returning <see cref="DateTime.MinValue" />.
    /// </summary>
    [Test]
    public void Read_InvalidString_ThrowsJsonException()
    {
        const string json = "{\"Value\":\"not-a-date\"}";

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<Wrapper>(json));
    }

    /// <summary>
    /// Serialization always emits the Bexio space-separated format, which is what the
    /// Bexio v2 endpoints expect on create / update payloads.
    /// </summary>
    [Test]
    public void Write_AlwaysEmitsBexioFormat()
    {
        var wrapper = new Wrapper(new DateTime(2026, 1, 16, 14, 20, 0));

        var json = JsonSerializer.Serialize(wrapper);

        json.ShouldContain("\"Value\":\"2026-01-16 14:20:00\"");
    }
}
