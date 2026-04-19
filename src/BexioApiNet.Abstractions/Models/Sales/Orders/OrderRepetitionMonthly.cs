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

namespace BexioApiNet.Abstractions.Models.Sales.Orders;

/// <summary>
///     Monthly repetition schedule — fires every <see cref="OrderRepetitionSchedule.Interval" />
///     months on the day described by <see cref="Schedule" />. Corresponds to the Bexio
///     <c>OrderRepetitionMonthly</c> schema (<c>type = "monthly"</c>).
/// </summary>
public sealed record OrderRepetitionMonthly : OrderRepetitionSchedule
{
    /// <inheritdoc />
    public override string Type => OrderRepetitionTypes.Monthly;

    /// <summary>
    ///     Day-of-month selector. One of the Bexio literals <c>fixed_day</c>, <c>week_day</c>,
    ///     <c>first_day</c> or <c>last_day</c>.
    /// </summary>
    [JsonPropertyName("schedule")]
    public string Schedule { get; init; } = string.Empty;
}
