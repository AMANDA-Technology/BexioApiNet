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

namespace BexioApiNet.Abstractions.Models.Timesheets.TimesheetStatus;

/// <summary>
/// Read-only lookup entry returned by <c>GET /2.0/timesheet_status</c>. Used as the
/// target of <c>status_id</c> on a <see cref="Timesheet.Timesheet" /> entry.
/// <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2ListTimeSheetStatus" />
/// </summary>
/// <param name="Id">Unique identifier of the status (read-only).</param>
/// <param name="Name">Display name of the status (e.g. <c>In Progress</c>).</param>
public sealed record TimesheetStatus(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name
);
