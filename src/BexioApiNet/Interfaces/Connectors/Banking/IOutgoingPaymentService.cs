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
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Banking;

/// <summary>
/// Service for managing outgoing payments in the purchase namespace.
/// <see href="https://docs.bexio.com/#tag/Outgoing-Payment">Outgoing Payment</see>
/// </summary>
public interface IOutgoingPaymentService
{
    /// <summary>
    /// List outgoing payments for a given bill.
    /// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPaymentList_GET">List Outgoing Payments</see>
    /// </summary>
    /// <param name="queryParameterOutgoingPayment">Query parameters — <c>bill_id</c> is mandatory per the Bexio spec.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the paged <see cref="OutgoingPaymentListResponse"/> envelope.</returns>
    public Task<ApiResult<OutgoingPaymentListResponse>> Get(QueryParameterOutgoingPayment queryParameterOutgoingPayment, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single outgoing payment by id.
    /// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPayment_GET">Get Outgoing Payment</see>
    /// </summary>
    /// <param name="id">Outgoing payment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the full <see cref="OutgoingPayment"/>.</returns>
    public Task<ApiResult<OutgoingPayment>> GetById(Guid id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new outgoing payment for a non-draft bill.
    /// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPayment_POST">Create Outgoing Payment</see>
    /// </summary>
    /// <param name="payload">Create view containing the payment details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the newly created <see cref="OutgoingPayment"/>.</returns>
    public Task<ApiResult<OutgoingPayment>> Create(OutgoingPaymentCreate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing outgoing payment. The target payment is identified by <see cref="OutgoingPaymentUpdate.PaymentId"/>
    /// inside the request body — the Bexio v4.0 PUT endpoint does not use <c>{id}</c> in the URL.
    /// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPayment_PUT">Edit Outgoing Payment</see>
    /// </summary>
    /// <param name="payload">Update view containing the payment id and the mutable fields.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="OutgoingPayment"/>.</returns>
    public Task<ApiResult<OutgoingPayment>> Update(OutgoingPaymentUpdate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an outgoing payment.
    /// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPayment_DELETE">Delete Outgoing Payment</see>
    /// </summary>
    /// <param name="id">Outgoing payment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> indicating success or failure.</returns>
    public Task<ApiResult<object>> Delete(Guid id, [Optional] CancellationToken cancellationToken);
}
