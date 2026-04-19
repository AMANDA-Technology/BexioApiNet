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
using BexioApiNet.Abstractions.Models.Sales.Positions;

namespace BexioApiNet.UnitTests.Sales.Positions;

/// <summary>
///     Round-trip tests for the polymorphic <see cref="Position" /> union. Each concrete variant
///     must serialize to the JSON shape Bexio expects (including the <c>type</c> discriminator) and
///     must deserialize back into the same concrete subtype when read from a Bexio response.
/// </summary>
[TestFixture]
public sealed class PositionJsonConverterTests
{
    /// <summary>
    ///     A <see cref="PositionArticle" /> round-trips through
    ///     <see cref="JsonSerializer" /> preserving every field and the <c>KbPositionArticle</c>
    ///     discriminator.
    /// </summary>
    [Test]
    public void PositionArticle_RoundTrips_PreservesAllFields()
    {
        var original = new PositionArticle
        {
            Id = 1,
            ParentId = null,
            Amount = "2.000000",
            UnitId = 3,
            AccountId = 4,
            TaxId = 5,
            Text = "Line item",
            UnitPrice = "99.500000",
            DiscountInPercent = "10.000000",
            Pos = "1",
            InternalPos = 1,
            IsOptional = false,
            ArticleId = 42
        };

        var json = JsonSerializer.Serialize<Position>(original);
        var roundTripped = JsonSerializer.Deserialize<Position>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"KbPositionArticle\""));
            Assert.That(roundTripped, Is.InstanceOf<PositionArticle>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     A <see cref="PositionCustom" /> round-trips preserving its fields and its
    ///     <c>KbPositionCustom</c> discriminator.
    /// </summary>
    [Test]
    public void PositionCustom_RoundTrips_PreservesAllFields()
    {
        var original = new PositionCustom
        {
            Id = 2,
            Amount = "1.000000",
            UnitId = 1,
            AccountId = 1,
            TaxId = 1,
            Text = "Custom line",
            UnitPrice = "50.000000",
            DiscountInPercent = null,
            Pos = "2",
            InternalPos = 2,
            IsOptional = null
        };

        var json = JsonSerializer.Serialize<Position>(original);
        var roundTripped = JsonSerializer.Deserialize<Position>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"KbPositionCustom\""));
            Assert.That(roundTripped, Is.InstanceOf<PositionCustom>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     A <see cref="PositionText" /> round-trips preserving its fields and its
    ///     <c>KbPositionText</c> discriminator.
    /// </summary>
    [Test]
    public void PositionText_RoundTrips_PreservesAllFields()
    {
        var original = new PositionText
        {
            Id = 3,
            Text = "Some paragraph",
            ShowPosNr = true,
            Pos = "3",
            InternalPos = 3,
            IsOptional = false
        };

        var json = JsonSerializer.Serialize<Position>(original);
        var roundTripped = JsonSerializer.Deserialize<Position>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"KbPositionText\""));
            Assert.That(roundTripped, Is.InstanceOf<PositionText>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     A <see cref="PositionSubposition" /> round-trips preserving its fields and its
    ///     <c>KbPositionSubposition</c> discriminator.
    /// </summary>
    [Test]
    public void PositionSubposition_RoundTrips_PreservesAllFields()
    {
        var original = new PositionSubposition
        {
            Id = 4,
            Text = "Group heading",
            Pos = "4",
            InternalPos = 4,
            ShowPosNr = true,
            IsOptional = false,
            TotalSum = "150.00",
            ShowPosPrices = true
        };

        var json = JsonSerializer.Serialize<Position>(original);
        var roundTripped = JsonSerializer.Deserialize<Position>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"KbPositionSubposition\""));
            Assert.That(roundTripped, Is.InstanceOf<PositionSubposition>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     A <see cref="PositionSubtotal" /> round-trips preserving its fields and its
    ///     <c>KbPositionSubtotal</c> discriminator.
    /// </summary>
    [Test]
    public void PositionSubtotal_RoundTrips_PreservesAllFields()
    {
        var original = new PositionSubtotal
        {
            Id = 5,
            Text = "Subtotal",
            Value = "300.00",
            InternalPos = 5,
            IsOptional = false
        };

        var json = JsonSerializer.Serialize<Position>(original);
        var roundTripped = JsonSerializer.Deserialize<Position>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"KbPositionSubtotal\""));
            Assert.That(roundTripped, Is.InstanceOf<PositionSubtotal>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     A <see cref="PositionPagebreak" /> round-trips preserving its fields and its
    ///     <c>KbPositionPagebreak</c> discriminator.
    /// </summary>
    [Test]
    public void PositionPagebreak_RoundTrips_PreservesAllFields()
    {
        var original = new PositionPagebreak
        {
            Id = 6,
            InternalPos = 6,
            IsOptional = false,
            Pagebreak = true
        };

        var json = JsonSerializer.Serialize<Position>(original);
        var roundTripped = JsonSerializer.Deserialize<Position>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"KbPositionPagebreak\""));
            Assert.That(roundTripped, Is.InstanceOf<PositionPagebreak>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     A <see cref="PositionDiscount" /> round-trips preserving its fields and its
    ///     <c>KbPositionDiscount</c> discriminator.
    /// </summary>
    [Test]
    public void PositionDiscount_RoundTrips_PreservesAllFields()
    {
        var original = new PositionDiscount
        {
            Id = 7,
            Text = "Loyalty discount",
            IsPercentual = true,
            Value = "10.000000",
            DiscountTotal = "15.00"
        };

        var json = JsonSerializer.Serialize<Position>(original);
        var roundTripped = JsonSerializer.Deserialize<Position>(json);

        Assert.Multiple(() =>
        {
            Assert.That(json, Does.Contain("\"type\":\"KbPositionDiscount\""));
            Assert.That(roundTripped, Is.InstanceOf<PositionDiscount>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     A mixed list of <see cref="Position" /> variants round-trips through serialization
    ///     preserving order, runtime types and field values — the path exercised by Bexio's document
    ///     responses, which return a heterogeneous <c>positions</c> array.
    /// </summary>
    [Test]
    public void MixedPositionList_RoundTrips_PreservesRuntimeTypes()
    {
        var original = new List<Position>
        {
            new PositionArticle { Id = 1, Text = "Article", Amount = "1.000000", UnitPrice = "10.000000" },
            new PositionText { Id = 2, Text = "Notes" },
            new PositionSubtotal { Id = 3, Text = "Running subtotal", Value = "10.00" },
            new PositionDiscount { Id = 4, Text = "-5%", IsPercentual = true, Value = "5.000000" }
        };

        var json = JsonSerializer.Serialize<IReadOnlyList<Position>>(original);
        var roundTripped = JsonSerializer.Deserialize<IReadOnlyList<Position>>(json);

        Assert.That(roundTripped, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(roundTripped!, Has.Count.EqualTo(4));
            Assert.That(roundTripped[0], Is.InstanceOf<PositionArticle>());
            Assert.That(roundTripped[1], Is.InstanceOf<PositionText>());
            Assert.That(roundTripped[2], Is.InstanceOf<PositionSubtotal>());
            Assert.That(roundTripped[3], Is.InstanceOf<PositionDiscount>());
            Assert.That(roundTripped, Is.EqualTo(original));
        });
    }

    /// <summary>
    ///     Deserializing a position payload whose <c>type</c> discriminator is unknown to the
    ///     converter must surface a <see cref="JsonException" /> rather than silently returning a
    ///     default instance — a payload Bexio should never emit, but worth failing fast on.
    /// </summary>
    [Test]
    public void UnknownDiscriminator_ThrowsJsonException()
    {
        const string payload = """{"type":"KbPositionUnknown","id":1}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Position>(payload));
    }

    /// <summary>
    ///     Deserializing a position payload missing the <c>type</c> discriminator altogether must
    ///     surface a <see cref="JsonException" /> rather than silently returning a default instance.
    /// </summary>
    [Test]
    public void MissingDiscriminator_ThrowsJsonException()
    {
        const string payload = """{"id":1,"text":"no type"}""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Position>(payload));
    }
}
