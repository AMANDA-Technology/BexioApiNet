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
using BexioApiNet.Abstractions.Models.Sales.Positions;
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;

namespace BexioApiNet.Interfaces.Connectors.Sales.Positions;

/// <summary>
/// Service for Bexio custom (default) position endpoints. Custom positions are free-form line
/// items not backed by an article and are accessed via
/// <c>/2.0/{kb_document_type}/{document_id}/kb_position_custom</c>.
/// Supported document types: <c>kb_invoice</c>, <c>kb_offer</c>, <c>kb_order</c>.
/// <see href="https://docs.bexio.com/#tag/Custom-positions" />
/// </summary>
public interface IDefaultPositionService
{
    /// <summary>
    /// List all custom positions on a document.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment (e.g. <c>kb_invoice</c>, <c>kb_offer</c>, <c>kb_order</c>).</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the list of custom positions.</returns>
    public Task<ApiResult<List<PositionCustom>>> Get(string documentType, int documentId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single custom position by identifier.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the custom position.</returns>
    public Task<ApiResult<PositionCustom>> GetById(string documentType, int documentId, int positionId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a custom position under a document.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="position">The create view model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the created custom position.</returns>
    public Task<ApiResult<PositionCustom>> Create(string documentType, int documentId, PositionCustomCreate position, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing custom position. Bexio uses <c>POST</c> (not <c>PUT</c>) for
    /// position updates.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="position">The update view model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated custom position.</returns>
    public Task<ApiResult<PositionCustom>> Update(string documentType, int documentId, int positionId, PositionCustomCreate position, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a custom position.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(string documentType, int documentId, int positionId, [Optional] CancellationToken cancellationToken);
}
