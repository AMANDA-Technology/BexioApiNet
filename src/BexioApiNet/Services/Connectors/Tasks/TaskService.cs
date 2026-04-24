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
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Tasks.Task;
using BexioApiNet.Abstractions.Models.Tasks.Task.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Tasks;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Tasks;

/// <inheritdoc cref="ITaskService" />
public sealed class TaskService : ConnectorService, ITaskService
{
    /// <summary>
    ///     The api endpoint version
    /// </summary>
    private const string ApiVersion = TaskConfiguration.ApiVersion;

    /// <summary>
    ///     The api request path
    /// </summary>
    private const string EndpointRoot = TaskConfiguration.EndpointRoot;

    /// <inheritdoc />
    public TaskService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<BexioTask>?>> Get([Optional] QueryParameterTask? queryParameterTask,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<BexioTask>?>($"{ApiVersion}/{EndpointRoot}",
            queryParameterTask?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null ||
            res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<BexioTask>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterTask?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<BexioTask>> GetById(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<BexioTask>($"{ApiVersion}/{EndpointRoot}/{id}", null,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<BexioTask>> Create(TaskCreate task, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<BexioTask, TaskCreate>(task, $"{ApiVersion}/{EndpointRoot}",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<BexioTask>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterTask? queryParameterTask, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<BexioTask>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/search",
            queryParameterTask?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<BexioTask>> Update(int id, TaskUpdate task,
        [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<BexioTask, TaskUpdate>(task, $"{ApiVersion}/{EndpointRoot}/{id}",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}