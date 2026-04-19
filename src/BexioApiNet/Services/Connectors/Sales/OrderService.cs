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
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Deliveries;
using BexioApiNet.Abstractions.Models.Sales.Invoices;
using BexioApiNet.Abstractions.Models.Sales.Orders;
using BexioApiNet.Abstractions.Models.Sales.Orders.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Sales;

/// <inheritdoc cref="IOrderService" />
public sealed class OrderService : ConnectorService, IOrderService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = OrderConfiguration.ApiVersion;

    /// <summary>
    /// The api request path
    /// </summary>
    private const string EndpointRoot = OrderConfiguration.EndpointRoot;

    /// <inheritdoc />
    public OrderService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Order>?>> Get([Optional] QueryParameterOrder? queryParameterOrder, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<Order>?>($"{ApiVersion}/{EndpointRoot}", queryParameterOrder?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null || res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<Order>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterOrder?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<Order>> GetById(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Order>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<byte[]>> GetPdf(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetBinaryAsync($"{ApiVersion}/{EndpointRoot}/{id}/pdf", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<OrderRepetition>> GetRepetition(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<OrderRepetition>($"{ApiVersion}/{EndpointRoot}/{id}/repetition", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Order>> Create(OrderCreate order, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Order, OrderCreate>(order, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Order>> Update(int id, OrderUpdate order, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Order, OrderUpdate>(order, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Order>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterOrder? queryParameterOrder, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<Order>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/search", queryParameterOrder?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Delivery>> CreateDeliveryFromOrder(int id, OrderConvertRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Delivery, OrderConvertRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/delivery", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Invoice>> CreateInvoiceFromOrder(int id, OrderConvertRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Invoice, OrderConvertRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/invoice", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<OrderRepetition>> CreateRepetition(int id, OrderRepetitionCreate repetition, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<OrderRepetition, OrderRepetitionCreate>(repetition, $"{ApiVersion}/{EndpointRoot}/{id}/repetition", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> DeleteRepetition(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}/repetition", cancellationToken);
    }
}
