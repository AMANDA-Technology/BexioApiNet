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
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces;

/// <summary>
/// Connection handler to call bexio REST API
/// </summary>
public interface IBexioConnectionHandler : IDisposable
{
    /// <summary>
    /// Simply get of any object type.
    /// </summary>
    /// <param name="requestPath">The api request path</param>
    /// <param name="queryParameter">Query parameter to append</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> GetAsync<TResult>(string requestPath, [Optional] QueryParameter? queryParameter, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Post any object type.
    /// </summary>
    /// <param name="payload">The payload, normally a create view of a given object type</param>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <typeparam name="TCreate">The create object view type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PostAsync<TResult, TCreate>(TCreate payload, string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Post any file content as multi part form request.
    /// </summary>
    /// <param name="files">A list of files</param>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PostMultiPartFileAsync<TResult>(List<Tuple<MemoryStream, string>> files, string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch all objects
    /// </summary>
    /// <param name="fetchedObjects">Count of the fetched objects</param>
    /// <param name="maxObjects">Max objects to fetch</param>
    /// <param name="requestPath">The api request path</param>
    /// <param name="queryParameter">Query parameter to append</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <returns></returns>
    public Task<List<TResult>> FetchAll<TResult>(int fetchedObjects, int maxObjects, string requestPath, QueryParameter? queryParameter, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an object
    /// </summary>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Put any object type. Used for full replacement updates (e.g. manual entries, payments, bills, expenses).
    /// </summary>
    /// <param name="payload">The payload, normally an update view of a given object type</param>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <typeparam name="TUpdate">The update object view type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PutAsync<TResult, TUpdate>(TUpdate payload, string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Patch any object type. Used for partial updates (e.g. currencies, files, employees, fictional users).
    /// </summary>
    /// <param name="payload">The payload, normally a patch view of a given object type</param>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <typeparam name="TPatch">The patch object view type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PatchAsync<TResult, TPatch>(TPatch payload, string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Post an action endpoint without a request body, returning a typed response
    /// (e.g. invoice issue/cancel/send, quote accept/reject, order delivery).
    /// </summary>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> PostActionAsync<TResult>(string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Post an action endpoint without a request body, returning no data
    /// (e.g. mark as sent, revert issue).
    /// </summary>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> PostActionAsync(string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a binary payload (e.g. invoice/quote/order PDFs, paystubs, file download or preview).
    /// </summary>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>An <see cref="ApiResult{T}"/> whose <c>Data</c> contains the response bytes on success, or null on failure.</returns>
    public Task<ApiResult<byte[]>> GetBinaryAsync(string requestPath, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Post a search query with a list of <see cref="SearchCriteria"/> to a Bexio <c>/search</c> endpoint.
    /// Optionally accepts a <see cref="QueryParameter"/> for pagination and sorting parameters
    /// (e.g. <c>limit</c>, <c>offset</c>, <c>order_by</c>).
    /// </summary>
    /// <param name="searchCriteria">The search criteria list sent as the JSON body</param>
    /// <param name="requestPath">The api request path</param>
    /// <param name="queryParameter">Optional query parameter to append to the URI</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result element type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<List<TResult>>> PostSearchAsync<TResult>(List<SearchCriteria> searchCriteria, string requestPath, [Optional] QueryParameter? queryParameter, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Post a list of payloads for bulk creation (e.g. contact bulk create).
    /// </summary>
    /// <param name="payloads">The payload list sent as the JSON body</param>
    /// <param name="requestPath">The api request path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result element type</typeparam>
    /// <typeparam name="TCreate">The create object view type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<List<TResult>>> PostBulkAsync<TResult, TCreate>(List<TCreate> payloads, string requestPath, [Optional] CancellationToken cancellationToken);
}
