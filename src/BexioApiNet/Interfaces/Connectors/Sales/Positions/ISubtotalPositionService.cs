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
using BexioApiNet.Abstractions.Enums.Sales;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Positions;
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;

namespace BexioApiNet.Interfaces.Connectors.Sales.Positions;

/// <summary>
///     Service for Bexio document subtotal positions. Subtotal positions print a running total
///     of preceding positions and are valid on quotes (<c>kb_offer</c>), orders (<c>kb_order</c>)
///     and invoices (<c>kb_invoice</c>) — not deliveries.
///     <see href="https://docs.bexio.com/#tag/Subtotal-positions" />
/// </summary>
public interface ISubtotalPositionService
{
    /// <summary>
    ///     List all subtotal positions for the given document.
    ///     <see href="https://docs.bexio.com/#tag/Subtotal-positions/operation/v2ListSubtotalPositions" />
    /// </summary>
    /// <param name="documentType">
    ///     The Bexio document type segment. Use <see cref="KbDocumentType" /> constants
    ///     (e.g. <see cref="KbDocumentType.Invoice" />).
    /// </param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the list of subtotal positions.</returns>
    public Task<ApiResult<List<PositionSubtotal>>> GetAll(string documentType, int documentId,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single subtotal position by identifier.
    ///     <see href="https://docs.bexio.com/#tag/Subtotal-positions/operation/v2ShowSubtotalPosition" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the subtotal position.</returns>
    public Task<ApiResult<PositionSubtotal>> GetById(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a new subtotal position under the given document.
    ///     <see href="https://docs.bexio.com/#tag/Subtotal-positions/operation/v2CreateSubtotalPosition" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="payload">The create payload — only <c>text</c> is accepted by the Bexio API.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the created subtotal position.</returns>
    public Task<ApiResult<PositionSubtotal>> Create(string documentType, int documentId,
        PositionSubtotalCreate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update an existing subtotal position. Bexio uses <c>POST</c> to the single-position path
    ///     (not <c>PUT</c>) for position updates.
    ///     <see href="https://docs.bexio.com/#tag/Subtotal-positions/operation/v2EditSubtotalPosition" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="payload">The update payload — only <c>text</c> is accepted by the Bexio API.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the updated subtotal position.</returns>
    public Task<ApiResult<PositionSubtotal>> Update(string documentType, int documentId, int positionId,
        PositionSubtotalCreate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a subtotal position.
    ///     <see href="https://docs.bexio.com/#tag/Subtotal-positions/operation/v2DeleteSubtotalPosition" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken);
}
