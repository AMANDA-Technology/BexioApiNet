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
using BexioApiNet.Abstractions.Models.Projects.Project;
using BexioApiNet.Abstractions.Models.Projects.Project.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Projects;

/// <summary>
/// Service for the Bexio projects endpoints. <see href="https://docs.bexio.com/#tag/Projects">Projects</see>
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// List all projects. <see href="https://docs.bexio.com/#tag/Projects/operation/v2ListProjects">List Projects</see>
    /// </summary>
    /// <param name="queryParameterProject">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">When <see langword="true"/>, transparently pages through all remaining results via <c>X-Total-Count</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the full page (or all pages) of projects.</returns>
    public Task<ApiResult<List<Project>?>> Get([Optional] QueryParameterProject? queryParameterProject, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single project by id. <see href="https://docs.bexio.com/#tag/Projects/operation/v2ShowProject">Show Project</see>
    /// </summary>
    /// <param name="id">The project id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The project matching the given id.</returns>
    public Task<ApiResult<Project>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a single project. <see href="https://docs.bexio.com/#tag/Projects/operation/v2CreateProject">Create Project</see>
    /// </summary>
    /// <param name="project">The project create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created project as returned by Bexio.</returns>
    public Task<ApiResult<Project>> Create(ProjectCreate project, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search projects by criteria. <see href="https://docs.bexio.com/#tag/Projects/operation/v2SearchProjects">Search Projects</see>
    /// Supported search fields: <c>name</c>, <c>contact_id</c>, <c>pr_state_id</c>.
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterProject">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching projects.</returns>
    public Task<ApiResult<List<Project>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterProject? queryParameterProject, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Edit an existing project. Bexio uses <c>POST /2.0/pr_project/{id}</c> (not <c>PUT</c>) for
    /// full-replacement updates — see <see href="https://docs.bexio.com/#tag/Projects/operation/v2EditProject">Edit Project</see>.
    /// </summary>
    /// <param name="id">The project id.</param>
    /// <param name="project">The project update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated project as returned by Bexio.</returns>
    public Task<ApiResult<Project>> Update(int id, ProjectUpdate project, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Archive a project. <see href="https://docs.bexio.com/#tag/Projects/operation/v2ArchiveProject">Archive Project</see>
    /// </summary>
    /// <param name="id">The project id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Archive(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Reactivate (unarchive) a previously archived project.
    /// <see href="https://docs.bexio.com/#tag/Projects/operation/v2UnarchiveProject">Unarchive Project</see>
    /// </summary>
    /// <param name="id">The project id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Reactivate(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Permanently delete a project. <see href="https://docs.bexio.com/#tag/Projects/operation/DeleteProject">Delete Project</see>
    /// </summary>
    /// <param name="id">The project id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
