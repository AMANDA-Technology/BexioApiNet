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
public interface IBexioConnectionHandler
{
    /// <summary>
    /// Simply get of any object type.
    /// </summary>
    /// <param name="requestPath">The api request path</param>
    /// <param name="queryParameter">Query parameter to append</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <typeparam name="TResult">The api result in the requested object type</typeparam>
    /// <returns></returns>
    public Task<ApiResult<TResult>> GetAsync<TResult>(string requestPath, [Optional] QueryParameter queryParameter, [Optional] CancellationToken cancellationToken);

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
    public Task<ApiResult<TResult>> PostMultiPartFileAsync<TResult>(List<FileInfo> files, string requestPath, [Optional] CancellationToken cancellationToken);

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
    public Task<List<TResult>> FetchAll<TResult>(int fetchedObjects, int maxObjects, string requestPath, QueryParameter queryParameter, [Optional] CancellationToken cancellationToken);
}
