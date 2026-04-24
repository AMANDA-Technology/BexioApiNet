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

namespace BexioApiNet.Abstractions.Models.Timesheets.Timesheet;

/// <summary>
/// Discriminated union over the three formats Bexio accepts for tracked time on a
/// <see cref="Timesheet" /> — <see cref="TimesheetDurationTracking" />,
/// <see cref="TimesheetRangeTracking" /> and <see cref="TimesheetStopwatchTracking" />.
/// The concrete subtype is selected from the <c>type</c> discriminator (<c>duration</c>,
/// <c>range</c>, <c>stopwatch</c>). Only <see cref="TimesheetDurationTracking" /> and
/// <see cref="TimesheetRangeTracking" /> may be submitted on create/update —
/// <see cref="TimesheetStopwatchTracking" /> is response-only.
/// <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2ListTimesheets" />
/// </summary>
[JsonConverter(typeof(TimesheetTrackingJsonConverter))]
public abstract record TimesheetTracking
{
    /// <summary>
    /// Discriminator string identifying the concrete tracking format
    /// (<c>duration</c>, <c>range</c> or <c>stopwatch</c>).
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}
