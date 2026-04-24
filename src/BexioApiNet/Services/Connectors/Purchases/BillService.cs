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
using BexioApiNet.Abstractions.Models.Purchases.Bills;
using BexioApiNet.Abstractions.Models.Purchases.Bills.Enums;
using BexioApiNet.Abstractions.Models.Purchases.Bills.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Purchases;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Purchases;

/// <inheritdoc cref="IBillService" />
public sealed class BillService : ConnectorService, IBillService
{
    /// <summary>
    /// The api endpoint version.
    /// </summary>
    private const string ApiVersion = BillConfiguration.ApiVersion;

    /// <summary>
    /// The api request path for bill resources.
    /// </summary>
    private const string EndpointRoot = BillConfiguration.EndpointRoot;

    /// <summary>
    /// The api request path for the document-number validation endpoint.
    /// </summary>
    private const string DocNumberEndpointRoot = BillConfiguration.DocNumberEndpointRoot;

    /// <inheritdoc />
    public BillService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<BillListResponse>> Get([Optional] QueryParameterBill? queryParameterBill, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<BillListResponse>($"{ApiVersion}/{EndpointRoot}", queryParameterBill?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Bill>> GetById(Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Bill>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<BillDocumentNumberResponse>> GetDocNumbers(string documentNo, [Optional] CancellationToken cancellationToken)
    {
        var queryParameter = new QueryParameter(new Dictionary<string, object>
        {
            ["document_no"] = documentNo
        });

        return await ConnectionHandler.GetAsync<BillDocumentNumberResponse>($"{ApiVersion}/{DocNumberEndpointRoot}", queryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Bill>> Create(BillCreate bill, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Bill, BillCreate>(bill, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Bill>> Actions(Guid id, BillActionRequest action, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Bill, BillActionRequest>(action, $"{ApiVersion}/{EndpointRoot}/{id}/actions", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Bill>> Update(Guid id, BillUpdate bill, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PutAsync<Bill, BillUpdate>(bill, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Bill>> UpdateBookings(Guid id, BillBookingStatus status, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PutAsync<Bill, object?>(null, $"{ApiVersion}/{EndpointRoot}/{id}/bookings/{status}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}
