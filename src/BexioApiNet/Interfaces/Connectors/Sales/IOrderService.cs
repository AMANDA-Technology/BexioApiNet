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
using BexioApiNet.Abstractions.Models.Sales.Invoices;
using BexioApiNet.Abstractions.Models.Sales.Orders;
using BexioApiNet.Abstractions.Models.Sales.Orders.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Sales;

/// <summary>
/// Service for the Bexio order endpoints. <see href="https://docs.bexio.com/#tag/Orders">Orders</see>
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// List all orders. <see href="https://docs.bexio.com/#tag/Orders/operation/v2ListOrders">List Orders</see>
    /// </summary>
    /// <param name="queryParameterOrder">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">When <see langword="true"/>, transparently pages through all remaining results via <c>X-Total-Count</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the full page (or all pages) of orders.</returns>
    public Task<ApiResult<List<Order>?>> Get([Optional] QueryParameterOrder? queryParameterOrder, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single order by id. <see href="https://docs.bexio.com/#tag/Orders/operation/v2ShowOrder">Show Order</see>
    /// </summary>
    /// <param name="id">The order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order including the <c>OrderWithDetails</c> fields when present.</returns>
    public Task<ApiResult<Order>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Download the order as PDF. <see href="https://docs.bexio.com/#tag/Orders/operation/v2ShowOrderPDF">Get Order PDF</see>
    /// </summary>
    /// <param name="id">The order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> whose <c>Data</c> is the PDF byte payload.</returns>
    public Task<ApiResult<byte[]>> GetPdf(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch the repetition schedule attached to an order.
    /// <see href="https://docs.bexio.com/#tag/Orders/operation/v2ShowOrderRepetition">Show Order Repetition</see>
    /// </summary>
    /// <param name="id">The order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The repetition payload for the order. Returns a 404 when no repetition is configured.</returns>
    public Task<ApiResult<OrderRepetition>> GetRepetition(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a single order. <see href="https://docs.bexio.com/#tag/Orders/operation/v2CreateOrder">Create Order</see>
    /// </summary>
    /// <param name="order">The order create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created order as returned by Bexio.</returns>
    public Task<ApiResult<Order>> Create(OrderCreate order, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Edit an existing order. Bexio uses <c>POST /2.0/kb_order/{id}</c> (not <c>PUT</c>) for
    /// full-replacement updates — see <see href="https://docs.bexio.com/#tag/Orders/operation/v2EditOrder">Edit Order</see>.
    /// </summary>
    /// <param name="id">The order id.</param>
    /// <param name="order">The order update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated order as returned by Bexio.</returns>
    public Task<ApiResult<Order>> Update(int id, OrderUpdate order, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an order. <see href="https://docs.bexio.com/#tag/Orders/operation/DeleteOrder">Delete Order</see>
    /// </summary>
    /// <param name="id">The order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search orders by criteria. <see href="https://docs.bexio.com/#tag/Orders/operation/v2SearchOrders">Search Orders</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterOrder">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching orders.</returns>
    public Task<ApiResult<List<Order>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterOrder? queryParameterOrder, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a delivery from an existing order.
    /// <see href="https://docs.bexio.com/#tag/Orders/operation/v2CreateDeliveryFromOrder">Create Delivery From Order</see>
    /// </summary>
    /// <param name="id">The source order id.</param>
    /// <param name="request">Optional subset of positions to carry over. When omitted, all positions are copied.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created delivery.</returns>
    public Task<ApiResult<Delivery>> CreateDeliveryFromOrder(int id, OrderConvertRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create an invoice from an existing order.
    /// <see href="https://docs.bexio.com/#tag/Orders/operation/v2CreateInvoiceFromOrder">Create Invoice From Order</see>
    /// </summary>
    /// <param name="id">The source order id.</param>
    /// <param name="request">Optional subset of positions to carry over. When omitted, all positions are copied.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created invoice.</returns>
    public Task<ApiResult<Invoice>> CreateInvoiceFromOrder(int id, OrderConvertRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create (or replace) a repetition schedule on an order.
    /// <see href="https://docs.bexio.com/#tag/Orders/operation/v2EditOrderRepetition">Edit Order Repetition</see>
    /// </summary>
    /// <param name="id">The order id.</param>
    /// <param name="repetition">The repetition payload (start/end plus the polymorphic repetition descriptor).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resulting repetition as returned by Bexio.</returns>
    public Task<ApiResult<OrderRepetition>> CreateRepetition(int id, OrderRepetitionCreate repetition, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete the repetition schedule attached to an order.
    /// <see href="https://docs.bexio.com/#tag/Orders/operation/v2DeleteOrderRepetition">Delete Order Repetition</see>
    /// </summary>
    /// <param name="id">The order id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> DeleteRepetition(int id, [Optional] CancellationToken cancellationToken);
}
