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

using BexioApiNet.Abstractions.Json;

namespace BexioApiNet.Abstractions.Models.Sales.Orders;

/// <summary>
///     Discriminated union over the four repetition schedules Bexio accepts on an
///     <see cref="OrderRepetition" /> payload — <see cref="OrderRepetitionDaily" />,
///     <see cref="OrderRepetitionWeekly" />, <see cref="OrderRepetitionMonthly" /> and
///     <see cref="OrderRepetitionYearly" />. The concrete subtype is selected from the
///     <c>type</c> discriminator (<c>daily</c>, <c>weekly</c>, <c>monthly</c>, <c>yearly</c>).
///     <see href="https://docs.bexio.com/#tag/Orders/operation/v2ShowOrderRepetition" />
/// </summary>
[JsonConverter(typeof(OrderRepetitionScheduleJsonConverter))]
public abstract record OrderRepetitionSchedule
{
    /// <summary>
    ///     Discriminator string identifying the concrete schedule (<c>daily</c>, <c>weekly</c>,
    ///     <c>monthly</c> or <c>yearly</c>).
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    ///     Multiplier applied to the schedule's natural period (e.g. <c>interval = 2</c> on a
    ///     weekly schedule means "every other week").
    /// </summary>
    [JsonPropertyName("interval")]
    public int Interval { get; init; }
}
