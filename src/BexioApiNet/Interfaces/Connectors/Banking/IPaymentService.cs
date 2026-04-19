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
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Banking;

/// <summary>
/// Service for managing payments in the banking namespace.
/// <see href="https://docs.bexio.com/#tag/Payments">Payments</see>
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Fetch a list of payments.
    /// <see href="https://docs.bexio.com/#tag/Payments/operation/NewFetchAllPayments">Fetch all payments</see>
    /// </summary>
    /// <param name="queryParameterPayment">Optional query parameters (pagination and filter).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the list of payments.</returns>
    public Task<ApiResult<List<Payment>?>> Get([Optional] QueryParameterPayment? queryParameterPayment, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single payment by its identifier.
    /// <see href="https://docs.bexio.com/#tag/Payments/operation/NewGetPayment">Get payment</see>
    /// </summary>
    /// <param name="paymentId">UUID of the payment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the payment.</returns>
    public Task<ApiResult<Payment>> GetById(Guid paymentId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new payment.
    /// <see href="https://docs.bexio.com/#tag/Payments/operation/NewCreatePayment">Create payment</see>
    /// </summary>
    /// <param name="payment">Create view for the payment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the created payment.</returns>
    public Task<ApiResult<Payment>> Create(PaymentCreate payment, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Cancel an existing payment. A payment can only be cancelled when its status
    /// is <c>downloaded</c>, <c>transferred</c> or <c>error</c>.
    /// <see href="https://docs.bexio.com/#tag/Payments/operation/NewCancelPayment">Cancel payment</see>
    /// </summary>
    /// <param name="paymentId">UUID of the payment to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the cancelled payment.</returns>
    public Task<ApiResult<Payment>> Cancel(Guid paymentId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing payment.
    /// <see href="https://docs.bexio.com/#tag/Payments/operation/NewUpdatePayment">Update payment</see>
    /// </summary>
    /// <param name="paymentId">UUID of the payment to update.</param>
    /// <param name="payment">Update view with the fields to change.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated payment.</returns>
    public Task<ApiResult<Payment>> Update(Guid paymentId, PaymentUpdate payment, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an existing payment permanently. This cannot be undone.
    /// <see href="https://docs.bexio.com/#tag/Payments/operation/NewDeletePayment">Delete payment</see>
    /// </summary>
    /// <param name="paymentId">UUID of the payment to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> indicating whether the delete succeeded.</returns>
    public Task<ApiResult<object>> Delete(Guid paymentId, [Optional] CancellationToken cancellationToken);
}
