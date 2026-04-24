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

namespace BexioApiNet.Abstractions.Models.Timesheets.Timesheet;

/// <summary>
/// Timesheet as returned by the Bexio timesheets endpoint.
/// <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2ListTimesheets" />
/// </summary>
/// <param name="Id">Unique timesheet identifier (read-only).</param>
/// <param name="UserId">Reference to a user object (the performer).</param>
/// <param name="StatusId">Reference to a timesheet status object.</param>
/// <param name="ClientServiceId">Reference to a business activity object.</param>
/// <param name="Text">Free-text description of the performed work.</param>
/// <param name="AllowableBill">When <see langword="true" />, the time is billable.</param>
/// <param name="Charge">Billable charge amount for the entry (monetary amount as string).</param>
/// <param name="ContactId">Reference to a contact object (the client).</param>
/// <param name="SubContactId">Reference to a sub-contact object (a contact below <see cref="ContactId" />).</param>
/// <param name="PrProjectId">Reference to a project object.</param>
/// <param name="PrPackageId">Reference to the project package.</param>
/// <param name="PrMilestoneId">Reference to the project milestone.</param>
/// <param name="TravelTime">Travel time associated with the entry (read-only).</param>
/// <param name="TravelCharge">Billable charge for the travel portion (read-only).</param>
/// <param name="TravelDistance">Distance travelled, in kilometres (read-only).</param>
/// <param name="EstimatedTime">Estimated time for the work in <c>HH:mm</c> format.</param>
/// <param name="Date">Date the timesheet was booked against (read-only).</param>
/// <param name="Duration">Total tracked duration in <c>HH:mm</c> format (read-only).</param>
/// <param name="Running">Indicates whether a stopwatch is currently running for this entry (read-only).</param>
/// <param name="Tracking">Tracked time payload — one of <see cref="TimesheetDurationTracking" />, <see cref="TimesheetRangeTracking" /> or <see cref="TimesheetStopwatchTracking" />.</param>
public sealed record Timesheet(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("status_id")] int? StatusId,
    [property: JsonPropertyName("client_service_id")] int ClientServiceId,
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("allowable_bill")] bool AllowableBill,
    [property: JsonPropertyName("charge")] string? Charge,
    [property: JsonPropertyName("contact_id")] int? ContactId,
    [property: JsonPropertyName("sub_contact_id")] int? SubContactId,
    [property: JsonPropertyName("pr_project_id")] int? PrProjectId,
    [property: JsonPropertyName("pr_package_id")] int? PrPackageId,
    [property: JsonPropertyName("pr_milestone_id")] int? PrMilestoneId,
    [property: JsonPropertyName("travel_time")] string? TravelTime,
    [property: JsonPropertyName("travel_charge")] string? TravelCharge,
    [property: JsonPropertyName("travel_distance")] int? TravelDistance,
    [property: JsonPropertyName("estimated_time")] string? EstimatedTime,
    [property: JsonPropertyName("date")] DateOnly? Date,
    [property: JsonPropertyName("duration")] string? Duration,
    [property: JsonPropertyName("running")] bool? Running,
    [property: JsonPropertyName("tracking")] TimesheetTracking? Tracking
);
