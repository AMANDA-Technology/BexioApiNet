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

using System.Runtime.InteropServices;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Timesheets.Timesheet;
using BexioApiNet.Abstractions.Models.Timesheets.Timesheet.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Timesheets;

/// <summary>
/// Service for the Bexio timesheets endpoints. <see href="https://docs.bexio.com/#tag/Timesheets">Timesheets</see>
/// </summary>
public interface ITimesheetService
{
    /// <summary>
    /// List timesheets. <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2ListTimesheets">List Timesheets</see>
    /// </summary>
    /// <param name="queryParameterTimesheet">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">When <see langword="true" />, transparently pages through all remaining results via <c>X-Total-Count</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> with the full page (or all pages) of timesheets.</returns>
    public Task<ApiResult<List<Timesheet>?>> Get([Optional] QueryParameterTimesheet? queryParameterTimesheet, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single timesheet by id. <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2ShowTimesheet">Show Timesheet</see>
    /// </summary>
    /// <param name="id">The timesheet id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The timesheet as returned by Bexio.</returns>
    public Task<ApiResult<Timesheet>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a single timesheet. <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2CreateTimesheet">Create Timesheet</see>
    /// </summary>
    /// <param name="timesheet">The timesheet create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created timesheet as returned by Bexio.</returns>
    public Task<ApiResult<Timesheet>> Create(TimesheetCreate timesheet, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search timesheets by criteria. <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2SearchTimesheets">Search Timesheets</see>
    /// Supported fields: <c>id</c>, <c>client_service_id</c>, <c>contact_id</c>, <c>user_id</c>, <c>pr_project_id</c>, <c>status_id</c>.
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterTimesheet">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching timesheets.</returns>
    public Task<ApiResult<List<Timesheet>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterTimesheet? queryParameterTimesheet, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update (edit) an existing timesheet. <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2EditTimesheet">Edit Timesheet</see>
    /// </summary>
    /// <param name="id">The timesheet id.</param>
    /// <param name="timesheet">The timesheet update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated timesheet as returned by Bexio.</returns>
    public Task<ApiResult<Timesheet>> Update(int id, TimesheetUpdate timesheet, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Permanently delete a timesheet. <see href="https://docs.bexio.com/#tag/Timesheets/operation/DeleteTimesheet">Delete Timesheet</see>
    /// </summary>
    /// <param name="id">The timesheet id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
