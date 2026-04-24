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

namespace BexioApiNet.Abstractions.Models.Timesheets.Timesheet.Views;

/// <summary>
/// Create view for a timesheet — body of <c>POST /2.0/timesheet</c>. Read-only fields
/// (<c>id</c>, <c>date</c>, <c>duration</c>, <c>running</c>, <c>travel_time</c>,
/// <c>travel_charge</c>, <c>travel_distance</c>) are intentionally omitted. The
/// <see cref="Tracking" /> payload must be either <see cref="TimesheetDurationTracking" />
/// or <see cref="TimesheetRangeTracking" /> — Bexio rejects the stopwatch variant on writes.
/// <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2CreateTimesheet" />
/// </summary>
/// <param name="UserId">Reference to a user object (the performer).</param>
/// <param name="ClientServiceId">Reference to a business activity object.</param>
/// <param name="AllowableBill">When <see langword="true" />, the time is billable.</param>
/// <param name="Tracking">Tracked time payload (<see cref="TimesheetDurationTracking" /> or <see cref="TimesheetRangeTracking" />).</param>
/// <param name="StatusId">Reference to a timesheet status object.</param>
/// <param name="Text">Free-text description of the performed work.</param>
/// <param name="Charge">Billable charge amount for the entry (monetary amount as string).</param>
/// <param name="ContactId">Reference to a contact object (the client).</param>
/// <param name="SubContactId">Reference to a sub-contact object (a contact below <see cref="ContactId" />).</param>
/// <param name="PrProjectId">Reference to a project object.</param>
/// <param name="PrPackageId">Reference to the project package.</param>
/// <param name="PrMilestoneId">Reference to the project milestone.</param>
/// <param name="EstimatedTime">Estimated time for the work in <c>HH:mm</c> format.</param>
public sealed record TimesheetCreate(
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("client_service_id")] int ClientServiceId,
    [property: JsonPropertyName("allowable_bill")] bool AllowableBill,
    [property: JsonPropertyName("tracking")] TimesheetTracking Tracking,
    [property: JsonPropertyName("status_id")] int? StatusId = null,
    [property: JsonPropertyName("text")] string? Text = null,
    [property: JsonPropertyName("charge")] string? Charge = null,
    [property: JsonPropertyName("contact_id")] int? ContactId = null,
    [property: JsonPropertyName("sub_contact_id")] int? SubContactId = null,
    [property: JsonPropertyName("pr_project_id")] int? PrProjectId = null,
    [property: JsonPropertyName("pr_package_id")] int? PrPackageId = null,
    [property: JsonPropertyName("pr_milestone_id")] int? PrMilestoneId = null,
    [property: JsonPropertyName("estimated_time")] string? EstimatedTime = null
);
