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
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Enums.MasterData;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.MasterData.Comments;
using BexioApiNet.Abstractions.Models.MasterData.Comments.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.MasterData;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.MasterData;

/// <inheritdoc cref="ICommentService" />
public sealed class CommentService : ConnectorService, ICommentService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = CommentConfiguration.ApiVersion;

    /// <summary>
    /// The trailing path segment after the document id
    /// </summary>
    private const string EndpointLeaf = CommentConfiguration.EndpointLeaf;

    /// <inheritdoc />
    public CommentService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Comment>?>> Get(KbDocumentType kbDocumentType, int documentId, [Optional] QueryParameterComment? queryParameterComment, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var requestPath = $"{ApiVersion}/{kbDocumentType.ToBexioString()}/{documentId}/{EndpointLeaf}";

        var res = await ConnectionHandler.GetAsync<List<Comment>?>(requestPath, queryParameterComment?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null || res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<Comment>(
            res.Data.Count,
            totalResults,
            requestPath,
            queryParameterComment?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<Comment>> GetById(KbDocumentType kbDocumentType, int documentId, int commentId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Comment>($"{ApiVersion}/{kbDocumentType.ToBexioString()}/{documentId}/{EndpointLeaf}/{commentId}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Comment>> Create(KbDocumentType kbDocumentType, int documentId, CommentCreate payload, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Comment, CommentCreate>(payload, $"{ApiVersion}/{kbDocumentType.ToBexioString()}/{documentId}/{EndpointLeaf}", cancellationToken);
    }
}
