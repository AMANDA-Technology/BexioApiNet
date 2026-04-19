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
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments;
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Banking;

/// <inheritdoc cref="IOutgoingPaymentService" />
public sealed class OutgoingPaymentService : ConnectorService, IOutgoingPaymentService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = OutgoingPaymentConfiguration.ApiVersion;

    /// <summary>
    /// The api request path
    /// </summary>
    private const string EndpointRoot = OutgoingPaymentConfiguration.EndpointRoot;

    /// <inheritdoc />
    public OutgoingPaymentService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<OutgoingPaymentListResponse>> Get(QueryParameterOutgoingPayment queryParameterOutgoingPayment, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<OutgoingPaymentListResponse>($"{ApiVersion}/{EndpointRoot}", queryParameterOutgoingPayment.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<OutgoingPayment>> GetById(Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<OutgoingPayment>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<OutgoingPayment>> Create(OutgoingPaymentCreate payload, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<OutgoingPayment, OutgoingPaymentCreate>(payload, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<OutgoingPayment>> Update(OutgoingPaymentUpdate payload, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PutAsync<OutgoingPayment, OutgoingPaymentUpdate>(payload, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}
