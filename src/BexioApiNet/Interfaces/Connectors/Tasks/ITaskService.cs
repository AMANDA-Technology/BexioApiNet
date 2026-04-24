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
using BexioApiNet.Abstractions.Models.Tasks.Task;
using BexioApiNet.Abstractions.Models.Tasks.Task.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Tasks;

/// <summary>
///     Service for the Bexio tasks endpoints. <see href="https://docs.bexio.com/#tag/Tasks">Tasks</see>
/// </summary>
public interface ITaskService
{
    /// <summary>
    ///     List all tasks. <see href="https://docs.bexio.com/#tag/Tasks/operation/v2ListTasks">List Tasks</see>
    /// </summary>
    /// <param name="queryParameterTask">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">
    ///     When <see langword="true" />, transparently pages through all remaining results via
    ///     <c>X-Total-Count</c>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> with the full page (or all pages) of tasks.</returns>
    public Task<ApiResult<List<BexioTask>?>> Get([Optional] QueryParameterTask? queryParameterTask,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single task by id. <see href="https://docs.bexio.com/#tag/Tasks/operation/v2ShowTask">Show Task</see>
    /// </summary>
    /// <param name="id">The task id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task as returned by Bexio.</returns>
    public Task<ApiResult<BexioTask>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a single task. <see href="https://docs.bexio.com/#tag/Tasks/operation/v2CreateTask">Create Task</see>
    /// </summary>
    /// <param name="task">The task create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created task as returned by Bexio.</returns>
    public Task<ApiResult<BexioTask>> Create(TaskCreate task, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search tasks by criteria. <see href="https://docs.bexio.com/#tag/Tasks/operation/v2SearchTasks">Search Tasks</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterTask">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching tasks.</returns>
    public Task<ApiResult<List<BexioTask>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterTask? queryParameterTask, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Edit an existing task. Bexio uses <c>POST /2.0/task/{id}</c> (not <c>PUT</c>) for
    ///     edits — see <see href="https://docs.bexio.com/#tag/Tasks/operation/v2EditTask">Edit Task</see>.
    /// </summary>
    /// <param name="id">The task id.</param>
    /// <param name="task">The task update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task as returned by Bexio.</returns>
    public Task<ApiResult<BexioTask>> Update(int id, TaskUpdate task, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a task. <see href="https://docs.bexio.com/#tag/Tasks/operation/DeleteTask">Delete Task</see>
    /// </summary>
    /// <param name="id">The task id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}