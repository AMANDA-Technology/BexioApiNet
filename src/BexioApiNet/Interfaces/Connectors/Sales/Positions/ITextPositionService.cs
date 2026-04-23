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

namespace BexioApiNet.Interfaces.Connectors.Sales.Positions;

/// <summary>
///     Service for Bexio document text positions.
///     <see href="https://docs.bexio.com/#tag/Text-positions" />
/// </summary>
public interface ITextPositionService
{
    /// <summary>
    ///     List all text positions for the given document.
    ///     <see href="https://docs.bexio.com/#tag/Text-positions/operation/v2ListKbPositionTexts" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment (e.g. <c>kb_invoice</c>, <c>kb_offer</c>).</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the list of text positions.</returns>
    public Task<ApiResult<List<PositionText>>> GetAll(string documentType, int documentId,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single text position by identifier.
    ///     <see href="https://docs.bexio.com/#tag/Text-positions/operation/v2ShowKbPositionText" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the text position.</returns>
    public Task<ApiResult<PositionText>> GetById(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a new text position under the given document.
    ///     <see href="https://docs.bexio.com/#tag/Text-positions/operation/v2CreateKbPositionText" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="position">The text position to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the created text position.</returns>
    public Task<ApiResult<PositionText>> Create(string documentType, int documentId, PositionText position,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update an existing text position. Bexio uses <c>POST</c> to the single-position path
    ///     (not <c>PUT</c>) for position updates.
    ///     <see href="https://docs.bexio.com/#tag/Text-positions/operation/v2EditKbPositionText" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="position">The updated text position payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the updated text position.</returns>
    public Task<ApiResult<PositionText>> Update(string documentType, int documentId, int positionId,
        PositionText position, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a text position.
    ///     <see href="https://docs.bexio.com/#tag/Text-positions/operation/v2DeleteKbPositionText" />
    /// </summary>
    /// <param name="documentType">The Bexio document type segment.</param>
    /// <param name="documentId">The parent document identifier.</param>
    /// <param name="positionId">The position identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(string documentType, int documentId, int positionId,
        [Optional] CancellationToken cancellationToken);
}