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
using BexioApiNet.Abstractions.Models.Accounting.CalendarYears;
using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Accounting;

/// <summary>
/// Service for working with accounting calendar years. <see href="https://docs.bexio.com/#tag/Calendar-Years">Calendar Years</see>
/// </summary>
public interface ICalendarYearService
{
    /// <summary>
    /// Fetch a list of calendar years. <see href="https://docs.bexio.com/#tag/Calendar-Years/operation/ListCalendarYears">List Calendar Years</see>
    /// </summary>
    /// <param name="queryParameterCalendarYear">Query parameter specific for calendar years</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<CalendarYear>?>> Get([Optional] QueryParameterCalendarYear? queryParameterCalendarYear, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single calendar year by id. <see href="https://docs.bexio.com/#tag/Calendar-Years/operation/ShowCalendarYear">Show Calendar Year</see>
    /// </summary>
    /// <param name="id">The calendar year id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<CalendarYear>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a calendar year. If a future year is passed, all years in between are generated using the provided settings. <see href="https://docs.bexio.com/#tag/Calendar-Years/operation/CreateCalendarYear">Create Calendar Year</see>
    /// </summary>
    /// <param name="calendarYearCreate">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<CalendarYear>>> Create(CalendarYearCreate calendarYearCreate, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search calendar years matching the provided criteria. <see href="https://docs.bexio.com/#tag/Calendar-Years/operation/SearchCalendarYears">Search Calendar Years</see>
    /// </summary>
    /// <param name="searchCriteria">The search criteria sent as the JSON body</param>
    /// <param name="queryParameterCalendarYear">Optional query parameter (e.g. limit / offset)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<CalendarYear>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterCalendarYear? queryParameterCalendarYear, [Optional] CancellationToken cancellationToken);
}
