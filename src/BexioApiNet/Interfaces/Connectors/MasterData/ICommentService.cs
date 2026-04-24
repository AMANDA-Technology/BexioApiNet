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
using BexioApiNet.Abstractions.Enums.MasterData;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.MasterData.Comments;
using BexioApiNet.Abstractions.Models.MasterData.Comments.Views;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
/// Service for fetching and creating comments on Bexio documents (offers, orders, invoices).
/// All routes follow the <c>/2.0/{kb_document_type}/{document_id}/comment</c> shape; the
/// discriminator lives in the URL.
/// <see href="https://docs.bexio.com/#tag/Comments">Comments</see>
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Fetch a list of comments for the given document.
    /// <see href="https://docs.bexio.com/#tag/Comments/operation/v2ListComments">List Comments</see>
    /// </summary>
    /// <param name="kbDocumentType">The document-type discriminator (offer, order, invoice).</param>
    /// <param name="documentId">The id of the parent document.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Comment>?>> Get(KbDocumentType kbDocumentType, int documentId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single comment for the given document.
    /// <see href="https://docs.bexio.com/#tag/Comments/operation/v2ShowComment">Show Comment</see>
    /// </summary>
    /// <param name="kbDocumentType">The document-type discriminator (offer, order, invoice).</param>
    /// <param name="documentId">The id of the parent document.</param>
    /// <param name="commentId">The id of the comment.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Comment>> GetById(KbDocumentType kbDocumentType, int documentId, int commentId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new comment on the given document.
    /// <see href="https://docs.bexio.com/#tag/Comments/operation/v2CreateComment">Create Comment</see>
    /// </summary>
    /// <param name="kbDocumentType">The document-type discriminator (offer, order, invoice).</param>
    /// <param name="documentId">The id of the parent document.</param>
    /// <param name="payload">Create view of the comment.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Comment>> Create(KbDocumentType kbDocumentType, int documentId, CommentCreate payload, [Optional] CancellationToken cancellationToken);
}
