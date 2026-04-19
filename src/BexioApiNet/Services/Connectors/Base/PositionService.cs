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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Interfaces;

namespace BexioApiNet.Services.Connectors.Base;

/// <summary>
/// Base service for Bexio document positions. All seven position types (item, default, discount,
/// text, subtotal, subposition, pagebreak) share the same polymorphic routing pattern
/// <c>/2.0/{kb_document_type}/{document_id}/kb_position_{type}/{position_id}</c> so the CRUD
/// implementation lives here and derived services only supply the <see cref="PositionType"/>
/// segment.
/// </summary>
public abstract class PositionService : ConnectorService
{
    /// <summary>
    /// The position-type URI segment that identifies the concrete position variant
    /// (e.g. <c>kb_position_article</c>, <c>kb_position_custom</c>).
    /// </summary>
    protected abstract string PositionType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionService"/> class.
    /// </summary>
    /// <param name="bexioConnectionHandler">The Bexio connection handler.</param>
    protected PositionService(IBexioConnectionHandler bexioConnectionHandler)
        : base(bexioConnectionHandler)
    {
    }

    /// <summary>
    /// Fetches all positions of the configured <see cref="PositionType"/> for a given document.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment (e.g. <c>kb_invoice</c>, <c>kb_offer</c>).</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TResult">The position view model type.</typeparam>
    /// <returns>The API result wrapping the list of positions.</returns>
    protected Task<ApiResult<List<TResult>>> GetAllPositionsAsync<TResult>(string documentType, int documentId, CancellationToken cancellationToken = default)
    {
        return ConnectionHandler.GetAsync<List<TResult>>($"2.0/{documentType}/{documentId}/{PositionType}", null, cancellationToken);
    }

    /// <summary>
    /// Fetches a single position by identifier.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TResult">The position view model type.</typeparam>
    /// <returns>The API result wrapping the position.</returns>
    protected Task<ApiResult<TResult>> GetPositionAsync<TResult>(string documentType, int documentId, int positionId, CancellationToken cancellationToken = default)
    {
        return ConnectionHandler.GetAsync<TResult>($"2.0/{documentType}/{documentId}/{PositionType}/{positionId}", null, cancellationToken);
    }

    /// <summary>
    /// Creates a new position under the given document.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="payload">The create view model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TResult">The position view model type returned by the API.</typeparam>
    /// <typeparam name="TCreate">The create view model type.</typeparam>
    /// <returns>The API result wrapping the created position.</returns>
    protected Task<ApiResult<TResult>> CreatePositionAsync<TResult, TCreate>(string documentType, int documentId, TCreate payload, CancellationToken cancellationToken = default)
    {
        return ConnectionHandler.PostAsync<TResult, TCreate>(payload, $"2.0/{documentType}/{documentId}/{PositionType}", cancellationToken);
    }

    /// <summary>
    /// Updates an existing position. Note that Bexio document-position updates are performed with
    /// HTTP <c>POST</c> to the single-position path (not <c>PUT</c>).
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="payload">The update view model.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <typeparam name="TResult">The position view model type returned by the API.</typeparam>
    /// <typeparam name="TUpdate">The update view model type.</typeparam>
    /// <returns>The API result wrapping the updated position.</returns>
    protected Task<ApiResult<TResult>> UpdatePositionAsync<TResult, TUpdate>(string documentType, int documentId, int positionId, TUpdate payload, CancellationToken cancellationToken = default)
    {
        return ConnectionHandler.PostAsync<TResult, TUpdate>(payload, $"2.0/{documentType}/{documentId}/{PositionType}/{positionId}", cancellationToken);
    }

    /// <summary>
    /// Deletes a position.
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API result for the delete operation.</returns>
    protected Task<ApiResult<object>> DeletePositionAsync(string documentType, int documentId, int positionId, CancellationToken cancellationToken = default)
    {
        return ConnectionHandler.Delete($"2.0/{documentType}/{documentId}/{PositionType}/{positionId}", cancellationToken);
    }
}
