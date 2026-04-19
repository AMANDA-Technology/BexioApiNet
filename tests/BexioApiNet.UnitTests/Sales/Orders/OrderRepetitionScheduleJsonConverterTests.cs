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
using BexioApiNet.Abstractions.Models.Sales.Orders;

namespace BexioApiNet.UnitTests.Sales.Orders;

/// <summary>
/// Round-trip tests for the polymorphic <see cref="OrderRepetitionSchedule" /> union. Each of
/// the four concrete schedules must serialize with the lowercase <c>type</c> discriminator the
/// Bexio API expects and must deserialize back into the same subtype when read from a response.
/// </summary>
[TestFixture]
public sealed class OrderRepetitionScheduleJsonConverterTests
{
    /// <summary>
    /// An <see cref="OrderRepetitionDaily" /> round-trips through <see cref="JsonSerializer" />
    /// carrying the <c>daily</c> discriminator and the <see cref="OrderRepetitionSchedule.Interval" />.
    /// </summary>
    [Test]
    public void OrderRepetitionDaily_RoundTrips_PreservesFields()
    {
        var original = new OrderRepetitionDaily { Interval = 3 };

        var json = JsonSerializer.Serialize<OrderRepetitionSchedule>(original);
        var roundTripped = JsonSerializer.Deserialize<OrderRepetitionSchedule>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"daily\""));
            Assert.That(json, Does.Contain("\"interval\":3"));
            Assert.That(roundTripped, Is.InstanceOf<OrderRepetitionDaily>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    /// An <see cref="OrderRepetitionWeekly" /> round-trips carrying the <c>weekly</c>
    /// discriminator, the interval and the weekday list.
    /// </summary>
    [Test]
    public void OrderRepetitionWeekly_RoundTrips_PreservesFields()
    {
        var original = new OrderRepetitionWeekly
        {
            Interval = 2,
            Weekdays = ["monday", "wednesday", "friday"]
        };

        var json = JsonSerializer.Serialize<OrderRepetitionSchedule>(original);
        var roundTripped = JsonSerializer.Deserialize<OrderRepetitionSchedule>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"weekly\""));
            Assert.That(json, Does.Contain("\"interval\":2"));
            Assert.That(json, Does.Contain("\"weekdays\":[\"monday\",\"wednesday\",\"friday\"]"));
            Assert.That(roundTripped, Is.InstanceOf<OrderRepetitionWeekly>());
        });

        var weekly = (OrderRepetitionWeekly)roundTripped!;
        Assert.Multiple(() =>
        {
            Assert.That(weekly.Interval, Is.EqualTo(original.Interval));
            Assert.That(weekly.Weekdays, Is.EqualTo(original.Weekdays).AsCollection);
        });
    }

    /// <summary>
    /// An <see cref="OrderRepetitionMonthly" /> round-trips carrying the <c>monthly</c>
    /// discriminator, the interval and the day-of-month selector.
    /// </summary>
    [Test]
    public void OrderRepetitionMonthly_RoundTrips_PreservesFields()
    {
        var original = new OrderRepetitionMonthly
        {
            Interval = 1,
            Schedule = "fixed_day"
        };

        var json = JsonSerializer.Serialize<OrderRepetitionSchedule>(original);
        var roundTripped = JsonSerializer.Deserialize<OrderRepetitionSchedule>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"monthly\""));
            Assert.That(json, Does.Contain("\"interval\":1"));
            Assert.That(json, Does.Contain("\"schedule\":\"fixed_day\""));
            Assert.That(roundTripped, Is.InstanceOf<OrderRepetitionMonthly>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    /// An <see cref="OrderRepetitionYearly" /> round-trips carrying the <c>yearly</c>
    /// discriminator and the interval.
    /// </summary>
    [Test]
    public void OrderRepetitionYearly_RoundTrips_PreservesFields()
    {
        var original = new OrderRepetitionYearly { Interval = 4 };

        var json = JsonSerializer.Serialize<OrderRepetitionSchedule>(original);
        var roundTripped = JsonSerializer.Deserialize<OrderRepetitionSchedule>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"yearly\""));
            Assert.That(json, Does.Contain("\"interval\":4"));
            Assert.That(roundTripped, Is.InstanceOf<OrderRepetitionYearly>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    /// Deserializing an order repetition payload whose <c>type</c> discriminator is unknown to
    /// the converter must surface a <see cref="JsonException" /> — a payload the Bexio API should
    /// never emit, but worth failing fast on.
    /// </summary>
    [Test]
    public void UnknownDiscriminator_ThrowsJsonException()
    {
        const string payload = """{"type":"hourly","interval":1}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OrderRepetitionSchedule>(payload));
    }

    /// <summary>
    /// Deserializing an order repetition payload missing the <c>type</c> discriminator altogether
    /// must surface a <see cref="JsonException" />.
    /// </summary>
    [Test]
    public void MissingDiscriminator_ThrowsJsonException()
    {
        const string payload = """{"interval":1}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<OrderRepetitionSchedule>(payload));
    }
}
