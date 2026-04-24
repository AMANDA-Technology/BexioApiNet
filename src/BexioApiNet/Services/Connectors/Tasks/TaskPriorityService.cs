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
using BexioApiNet.Abstractions.Models.Tasks.TaskPriority;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Tasks;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Tasks;

/// <inheritdoc cref="ITaskPriorityService" />
public sealed class TaskPriorityService : ConnectorService, ITaskPriorityService
{
    /// <summary>
    ///     The api endpoint version
    /// </summary>
    private const string ApiVersion = TaskPriorityConfiguration.ApiVersion;

    /// <summary>
    ///     The api request path
    /// </summary>
    private const string EndpointRoot = TaskPriorityConfiguration.EndpointRoot;

    /// <inheritdoc />
    public TaskPriorityService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<TaskPriority>?>> Get([Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<List<TaskPriority>?>($"{ApiVersion}/{EndpointRoot}", null,
            cancellationToken);
    }
}