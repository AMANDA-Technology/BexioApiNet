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
using BexioApiNet.Abstractions.Models.Items.StockLocations;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Items;

/// <summary>
/// Service for stock locations. Bexio exposes these under the <c>/2.0/stock</c> routes.
/// <see href="https://docs.bexio.com/#tag/Stock-locations"/>
/// </summary>
public interface IStockLocationService
{
    /// <summary>
    /// Fetch a list of stock locations. <see href="https://docs.bexio.com/#tag/Stock-locations/operation/v2ListStockLocations"/>
    /// </summary>
    /// <param name="queryParameterStockLocation">Query parameters (pagination / ordering)</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<StockLocation>?>> Get([Optional] QueryParameterStockLocation? queryParameterStockLocation, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search stock locations. <see href="https://docs.bexio.com/#tag/Stock-locations/operation/v2SearchStockLocations"/>
    /// Supported search fields: <c>name</c>.
    /// </summary>
    /// <param name="searchCriteria">Search criteria sent as JSON body</param>
    /// <param name="queryParameterStockLocation">Optional pagination / ordering parameters appended to the URI</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<StockLocation>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterStockLocation? queryParameterStockLocation, [Optional] CancellationToken cancellationToken);
}
