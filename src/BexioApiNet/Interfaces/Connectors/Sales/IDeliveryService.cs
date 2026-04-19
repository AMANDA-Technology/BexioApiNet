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
using BexioApiNet.Abstractions.Models.Sales.Deliveries;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Sales;

/// <summary>
///     Service for the Bexio delivery endpoints. <see href="https://docs.bexio.com/#tag/Deliveries">Deliveries</see>
/// </summary>
public interface IDeliveryService
{
    /// <summary>
    ///     List all deliveries.
    ///     <see href="https://docs.bexio.com/#tag/Deliveries/operation/v2ListDeliveries">List Deliveries</see>
    /// </summary>
    /// <param name="queryParameterDelivery">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">
    ///     When <see langword="true" />, transparently pages through all remaining results via
    ///     <c>X-Total-Count</c>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> with the full page (or all pages) of deliveries.</returns>
    public Task<ApiResult<List<Delivery>?>> Get([Optional] QueryParameterDelivery? queryParameterDelivery,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single delivery by id.
    ///     <see href="https://docs.bexio.com/#tag/Deliveries/operation/v2ShowDelivery">Show Delivery</see>
    /// </summary>
    /// <param name="id">The delivery id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delivery including the <c>DeliveryWithDetails</c> fields when present.</returns>
    public Task<ApiResult<Delivery>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Issue a delivery. The delivery must be in the draft status.
    ///     <see href="https://docs.bexio.com/#tag/Deliveries/operation/v2IssueDelivery">Issue Delivery</see>
    /// </summary>
    /// <param name="id">The delivery id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Issue(int id, [Optional] CancellationToken cancellationToken);
}
