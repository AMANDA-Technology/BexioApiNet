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
/// Service for the Bexio sub-position endpoints.
/// Sub-positions group other positions under a shared heading and running subtotal.
/// They are valid on quotes, orders and invoices.
/// <see href="https://docs.bexio.com/#tag/Sub-positions"/>
/// </summary>
public interface ISubPositionService
{
    /// <summary>
    /// List all sub-positions for a document.
    /// <see href="https://docs.bexio.com/#tag/Sub-positions/operation/v2ListSubpositionPositions"/>
    /// </summary>
    /// <param name="documentType">
    /// The Bexio document type path segment. Use <see cref="KbDocumentType"/> constants
    /// (e.g. <see cref="KbDocumentType.Invoice"/>).
    /// </param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the list of sub-positions for the document.</returns>
    public Task<ApiResult<List<PositionSubposition>>> Get(string documentType, int documentId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single sub-position by identifier.
    /// <see href="https://docs.bexio.com/#tag/Sub-positions/operation/v2ShowSubpositionPosition"/>
    /// </summary>
    /// <param name="documentType">The Bexio document type path segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the sub-position.</returns>
    public Task<ApiResult<PositionSubposition>> GetById(string documentType, int documentId, int positionId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a sub-position under a document.
    /// <see href="https://docs.bexio.com/#tag/Sub-positions/operation/v2CreateSubpositionPosition"/>
    /// </summary>
    /// <param name="documentType">The Bexio document type path segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="payload">The create payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the created sub-position.</returns>
    public Task<ApiResult<PositionSubposition>> Create(string documentType, int documentId, PositionSubpositionCreate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Edit an existing sub-position. Bexio uses <c>POST</c> to the single-position path for updates.
    /// <see href="https://docs.bexio.com/#tag/Sub-positions/operation/v2EditSubpositionPosition"/>
    /// </summary>
    /// <param name="documentType">The Bexio document type path segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="payload">The update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated sub-position.</returns>
    public Task<ApiResult<PositionSubposition>> Update(string documentType, int documentId, int positionId, PositionSubpositionUpdate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a sub-position.
    /// <see href="https://docs.bexio.com/#tag/Sub-positions/operation/v2DeleteSubpositionPosition"/>
    /// </summary>
    /// <param name="documentType">The Bexio document type path segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(string documentType, int documentId, int positionId, [Optional] CancellationToken cancellationToken);
}
