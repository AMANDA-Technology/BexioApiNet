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
using BexioApiNet.Abstractions.Models.Timesheets.Timesheet;

namespace BexioApiNet.UnitTests.Timesheets;

/// <summary>
/// Round-trip tests for the polymorphic <see cref="TimesheetTracking" /> union. Each of the
/// three concrete variants must serialize with the lowercase <c>type</c> discriminator the
/// Bexio API expects and must deserialize back into the same subtype when read from a response.
/// </summary>
[TestFixture]
[Category("Unit")]
public sealed class TimesheetTrackingJsonConverterTests
{
    /// <summary>
    /// A <see cref="TimesheetDurationTracking" /> round-trips carrying the <c>duration</c>
    /// discriminator, the date, and the duration string.
    /// </summary>
    [Test]
    public void TimesheetDurationTracking_RoundTrips_PreservesFields()
    {
        var original = new TimesheetDurationTracking(new DateOnly(2019, 5, 20), "01:40");

        var json = JsonSerializer.Serialize<TimesheetTracking>(original);
        var roundTripped = JsonSerializer.Deserialize<TimesheetTracking>(json);

        json.ShouldContain("\"type\":\"duration\"");
        json.ShouldContain("\"date\":\"2019-05-20\"");
        json.ShouldContain("\"duration\":\"01:40\"");
        roundTripped.ShouldBeOfType<TimesheetDurationTracking>();
        roundTripped.ShouldBe(original);
    }

    /// <summary>
    /// A <see cref="TimesheetRangeTracking" /> round-trips carrying the <c>range</c>
    /// discriminator, start and end timestamps.
    /// </summary>
    [Test]
    public void TimesheetRangeTracking_RoundTrips_PreservesFields()
    {
        var original = new TimesheetRangeTracking("2019-05-20 14:22:48", "2019-05-20 16:13:25");

        var json = JsonSerializer.Serialize<TimesheetTracking>(original);
        var roundTripped = JsonSerializer.Deserialize<TimesheetTracking>(json);

        json.ShouldContain("\"type\":\"range\"");
        json.ShouldContain("\"start\":\"2019-05-20 14:22:48\"");
        json.ShouldContain("\"end\":\"2019-05-20 16:13:25\"");
        roundTripped.ShouldBeOfType<TimesheetRangeTracking>();
        roundTripped.ShouldBe(original);
    }

    /// <summary>
    /// A <see cref="TimesheetStopwatchTracking" /> (response-only) round-trips carrying the
    /// <c>stopwatch</c> discriminator and the elapsed duration.
    /// </summary>
    [Test]
    public void TimesheetStopwatchTracking_RoundTrips_PreservesFields()
    {
        var original = new TimesheetStopwatchTracking("01:40");

        var json = JsonSerializer.Serialize<TimesheetTracking>(original);
        var roundTripped = JsonSerializer.Deserialize<TimesheetTracking>(json);

        json.ShouldContain("\"type\":\"stopwatch\"");
        json.ShouldContain("\"duration\":\"01:40\"");
        roundTripped.ShouldBeOfType<TimesheetStopwatchTracking>();
        roundTripped.ShouldBe(original);
    }

    /// <summary>
    /// Deserializing a tracking payload whose <c>type</c> discriminator is unknown to
    /// the converter must surface a <see cref="JsonException" /> — a payload the Bexio API
    /// should never emit, but worth failing fast on.
    /// </summary>
    [Test]
    public void UnknownDiscriminator_ThrowsJsonException()
    {
        const string payload = """{"type":"pomodoro","duration":"00:25"}""";

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TimesheetTracking>(payload));
    }

    /// <summary>
    /// Deserializing a tracking payload missing the <c>type</c> discriminator altogether
    /// must surface a <see cref="JsonException" />.
    /// </summary>
    [Test]
    public void MissingDiscriminator_ThrowsJsonException()
    {
        const string payload = """{"duration":"01:40"}""";

        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<TimesheetTracking>(payload));
    }
}
