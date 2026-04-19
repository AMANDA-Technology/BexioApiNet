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
using BexioApiNet.Abstractions.Models.Banking.Payments;
using BexioApiNet.Abstractions.Models.Banking.Payments.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Banking;

/// <inheritdoc cref="IPaymentService" />
public sealed class PaymentService : ConnectorService, IPaymentService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = PaymentConfiguration.ApiVersion;

    /// <summary>
    /// The api request path
    /// </summary>
    private const string EndpointRoot = PaymentConfiguration.EndpointRoot;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentService"/> class.
    /// </summary>
    /// <param name="bexioConnectionHandler">Bexio connection handler.</param>
    public PaymentService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Payment>?>> Get([Optional] QueryParameterPayment? queryParameterPayment, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.GetAsync<List<Payment>?>($"{ApiVersion}/{EndpointRoot}", queryParameterPayment?.QueryParameter, cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<Payment>> GetById(Guid paymentId, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.GetAsync<Payment>($"{ApiVersion}/{EndpointRoot}/{paymentId}", null, cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<Payment>> Create(PaymentCreate payment, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.PostAsync<Payment, PaymentCreate>(payment, $"{ApiVersion}/{EndpointRoot}", cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<Payment>> Cancel(Guid paymentId, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.PostActionAsync<Payment>($"{ApiVersion}/{EndpointRoot}/{paymentId}/cancel", cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<Payment>> Update(Guid paymentId, PaymentUpdate payment, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.PutAsync<Payment, PaymentUpdate>(payment, $"{ApiVersion}/{EndpointRoot}/{paymentId}", cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(Guid paymentId, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{paymentId}", cancellationToken);
}
