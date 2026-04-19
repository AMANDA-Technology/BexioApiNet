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
using BexioApiNet.Abstractions.Models.Accounting.Reports;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Accounting;

/// <summary>
/// Service for reading accounting reports such as the journal. <see href="https://docs.bexio.com/#tag/Reports">Reports</see>
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Get the accounting journal (list of all journal bookings). <see href="https://docs.bexio.com/#tag/Reports/operation/ListJournalEntries">List Journal Entries</see>
    /// </summary>
    /// <param name="queryParameterJournal">Query parameters (date range, account filter, paging).</param>
    /// <param name="autoPage">Fetch all possible results by walking pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> containing the journal entries returned by the API.</returns>
    public Task<ApiResult<List<Journal>>> GetJournal([Optional] QueryParameterJournal? queryParameterJournal, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);
}
