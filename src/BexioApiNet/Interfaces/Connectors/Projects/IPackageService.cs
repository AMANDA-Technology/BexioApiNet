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
using BexioApiNet.Abstractions.Models.Projects.Packages;
using BexioApiNet.Abstractions.Models.Projects.Packages.Views;

namespace BexioApiNet.Interfaces.Connectors.Projects;

/// <summary>
///     Service for Bexio project work packages. All routes are nested under
///     <c>/3.0/projects/{project_id}/packages</c>, so every method accepts the parent project
///     identifier. <see href="https://docs.bexio.com/#tag/Projects">Projects</see>
/// </summary>
public interface IPackageService
{
    /// <summary>
    ///     Fetch all work packages for a given project.
    ///     <see href="https://docs.bexio.com/#tag/Projects/operation/ListWorkPackages">List Work Packages</see>
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the list of work packages for the project.</returns>
    public Task<ApiResult<List<Package>?>> GetAsync(int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Fetch a single work package by identifier for a given project.
    ///     <see href="https://docs.bexio.com/#tag/Projects/operation/ShowWorkPackage">Show Work Package</see>
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="id">The work package identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the work package.</returns>
    public Task<ApiResult<Package>> GetByIdAsync(int projectId, int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Create a new work package for a given project.
    ///     <see href="https://docs.bexio.com/#tag/Projects/operation/CreateWorkPackage">Create Work Package</see>
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="model">The create view payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the created work package.</returns>
    public Task<ApiResult<Package>> CreateAsync(int projectId, PackageCreate model,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Partially update an existing work package via HTTP <c>PATCH</c>.
    ///     <see href="https://docs.bexio.com/#tag/Projects/operation/EditWorkPackage">Edit Work Package</see>
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="id">The work package identifier.</param>
    /// <param name="model">The patch view payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the updated work package.</returns>
    public Task<ApiResult<Package>> PatchAsync(int projectId, int id, PackagePatch model,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Delete a work package permanently. This action cannot be undone.
    ///     <see href="https://docs.bexio.com/#tag/Projects/operation/DeleteWorkPackage">Delete Work Package</see>
    /// </summary>
    /// <param name="projectId">The parent project identifier.</param>
    /// <param name="id">The work package identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> DeleteAsync(int projectId, int id, CancellationToken cancellationToken = default);
}
