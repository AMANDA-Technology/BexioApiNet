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
using BexioApiNet.Abstractions.Models.Purchases.PurchaseOrders;
using BexioApiNet.Abstractions.Models.Purchases.PurchaseOrders.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Purchases;

/// <summary>
/// Service for managing purchase orders in the Bexio purchase namespace (v3.0).
/// <see href="https://docs.bexio.com/#tag/Purchase-Orders">Purchase Orders</see>
/// </summary>
public interface IPurchaseOrderService
{
    /// <summary>
    /// List purchase orders.
    /// <see href="https://docs.bexio.com/#tag/Purchase-Orders">List Purchase Orders</see>
    /// </summary>
    /// <param name="queryParameter">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the list of purchase orders.</returns>
    public Task<ApiResult<List<PurchaseOrder>?>> Get([Optional] QueryParameter? queryParameter, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single purchase order by id.
    /// <see href="https://docs.bexio.com/#tag/Purchase-Orders">Get Purchase Order</see>
    /// </summary>
    /// <param name="id">Purchase order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the full <see cref="PurchaseOrder"/>.</returns>
    public Task<ApiResult<PurchaseOrder>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new purchase order.
    /// <see href="https://docs.bexio.com/#tag/Purchase-Orders">Create Purchase Order</see>
    /// </summary>
    /// <param name="purchaseOrder">Create view containing the purchase order details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the newly created <see cref="PurchaseOrder"/>.</returns>
    public Task<ApiResult<PurchaseOrder>> Create(PurchaseOrderCreate purchaseOrder, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing purchase order. Bexio v3.0 uses <c>PUT /3.0/purchase_orders/{id}</c>
    /// for full-replacement updates.
    /// <see href="https://docs.bexio.com/#tag/Purchase-Orders/operation/v3PurchaseOrderUpdate">Update Purchase Order</see>
    /// </summary>
    /// <param name="id">Purchase order identifier to update.</param>
    /// <param name="purchaseOrder">Update view containing the full purchase order state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="PurchaseOrder"/>.</returns>
    public Task<ApiResult<PurchaseOrder>> Update(int id, PurchaseOrderUpdate purchaseOrder, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a purchase order.
    /// <see href="https://docs.bexio.com/#tag/Purchase-Orders">Delete Purchase Order</see>
    /// </summary>
    /// <param name="id">Purchase order identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> indicating success or failure.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
