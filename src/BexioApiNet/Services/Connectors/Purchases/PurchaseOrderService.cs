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
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Purchases;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Purchases;

/// <inheritdoc cref="IPurchaseOrderService" />
public sealed class PurchaseOrderService : ConnectorService, IPurchaseOrderService
{
    /// <summary>
    /// The api endpoint version.
    /// </summary>
    private const string ApiVersion = PurchaseOrderConfiguration.ApiVersion;

    /// <summary>
    /// The api request path for purchase order resources.
    /// </summary>
    private const string EndpointRoot = PurchaseOrderConfiguration.EndpointRoot;

    /// <inheritdoc />
    public PurchaseOrderService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<PurchaseOrder>?>> Get([Optional] QueryParameter? queryParameter, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<List<PurchaseOrder>?>($"{ApiVersion}/{EndpointRoot}", queryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<PurchaseOrder>> GetById(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<PurchaseOrder>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<PurchaseOrder>> Create(PurchaseOrderCreate purchaseOrder, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<PurchaseOrder, PurchaseOrderCreate>(purchaseOrder, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<PurchaseOrder>> Update(int id, PurchaseOrderUpdate purchaseOrder, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<PurchaseOrder, PurchaseOrderUpdate>(purchaseOrder, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}
