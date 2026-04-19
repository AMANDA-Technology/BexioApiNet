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
using BexioApiNet.Abstractions.Models.Accounting.BusinessYears;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Accounting;

/// <summary>
///     Service for getting business year entries in accounting namespace.
///     <see href="https://docs.bexio.com/#tag/Business-Years">Business Years</see>
/// </summary>
public interface IBusinessYearService
{
    /// <summary>
    ///     Get a list of business years.
    ///     <see href="https://docs.bexio.com/#tag/Business-Years/operation/ListBusinessYears">List Business Years</see>
    /// </summary>
    /// <param name="queryParameterBusinessYear">Query parameter specific for business years</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<BusinessYear>>> Get([Optional] QueryParameterBusinessYear? queryParameterBusinessYear,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Get a single business year by id.
    ///     <see href="https://docs.bexio.com/#tag/Business-Years/operation/ShowBusinessYear">Show Business Year</see>
    /// </summary>
    /// <param name="id">The id of the business year</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<BusinessYear>> GetById(int id, [Optional] CancellationToken cancellationToken);
}
