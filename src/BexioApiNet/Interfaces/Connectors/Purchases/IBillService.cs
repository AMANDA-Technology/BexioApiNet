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
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Purchases;

/// <summary>
/// Service for managing bills in the Bexio purchase namespace (v4.0).
/// <see href="https://docs.bexio.com/#tag/Bills">Bills</see>
/// </summary>
public interface IBillService
{
    /// <summary>
    /// List bills.
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiBillsList_GET">List Bills</see>
    /// </summary>
    /// <param name="queryParameterBill">Optional query parameters for pagination, sorting and filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the paged <see cref="BillListResponse"/> envelope.</returns>
    public Task<ApiResult<BillListResponse>> Get([Optional] QueryParameterBill? queryParameterBill, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single bill by id.
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiBills_GET">Get Bill</see>
    /// </summary>
    /// <param name="id">Bill identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the full <see cref="Bill"/>.</returns>
    public Task<ApiResult<Bill>> GetById(Guid id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Validate a proposed bill document number. Returns the next available number when
    /// the proposed one is not unique among non-draft bills.
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiPurchaseDocumentNumbers_GET">Validate Document Number</see>
    /// </summary>
    /// <param name="documentNo">Proposed document number to validate (max 255 characters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the <see cref="BillDocumentNumberResponse"/>.</returns>
    public Task<ApiResult<BillDocumentNumberResponse>> GetDocNumbers(string documentNo, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new bill.
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiBills_POST">Create Bill</see>
    /// </summary>
    /// <param name="bill">Create view containing the bill details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the newly created <see cref="Bill"/>.</returns>
    public Task<ApiResult<Bill>> Create(BillCreate bill, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Execute a bill action (e.g. <see cref="BillAction.DUPLICATE"/>).
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiBillActions_POST">Execute Bill Action</see>
    /// </summary>
    /// <param name="id">Bill identifier the action is executed for.</param>
    /// <param name="action">Action payload identifying the operation to perform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the resulting <see cref="Bill"/> (for <c>DUPLICATE</c> this is the new duplicate).</returns>
    public Task<ApiResult<Bill>> Actions(Guid id, BillActionRequest action, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing bill. Bexio v4.0 uses PUT for full-replacement updates. When the
    /// bill is not in <c>DRAFT</c> only <c>file_id</c> and <c>payment</c> are actually persisted.
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiBills_PUT">Update Bill</see>
    /// </summary>
    /// <param name="id">Bill identifier to update.</param>
    /// <param name="bill">Update view containing the full bill state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="Bill"/>.</returns>
    public Task<ApiResult<Bill>> Update(Guid id, BillUpdate bill, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Transition a bill's booking status between <c>DRAFT</c> and <c>BOOKED</c>.
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiBillBookings_PUT">Update Bill Status</see>
    /// </summary>
    /// <param name="id">Bill identifier.</param>
    /// <param name="status">Target booking status (<see cref="BillBookingStatus.DRAFT"/> or <see cref="BillBookingStatus.BOOKED"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="Bill"/>.</returns>
    public Task<ApiResult<Bill>> UpdateBookings(Guid id, BillBookingStatus status, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a bill. Only allowed for bills in status <c>DRAFT</c>.
    /// <see href="https://docs.bexio.com/#tag/Bills/operation/ApiBills_DELETE">Delete Bill</see>
    /// </summary>
    /// <param name="id">Bill identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> indicating success or failure.</returns>
    public Task<ApiResult<object>> Delete(Guid id, [Optional] CancellationToken cancellationToken);
}
