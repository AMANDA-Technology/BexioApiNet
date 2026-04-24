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
using BexioApiNet.Abstractions.Models.MasterData.FictionalUsers;
using BexioApiNet.Abstractions.Models.MasterData.FictionalUsers.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
///     Service for managing Bexio fictional user accounts.
///     <see href="https://docs.bexio.com/#tag/Fictional-User-Management">Fictional User Management</see>
/// </summary>
public interface IFictionalUserService
{
    /// <summary>
    ///     Get a list of fictional users.
    ///     <see href="https://docs.bexio.com/#tag/Fictional-User-Management/operation/v3ListFictionalUsers">
    ///         List Fictional Users
    ///     </see>
    /// </summary>
    /// <param name="queryParameterFictionalUser">Optional pagination query parameters.</param>
    /// <param name="autoPage">Fetch all remaining pages automatically when <c>true</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the list of fictional users.</returns>
    public Task<ApiResult<List<FictionalUser>>> Get([Optional] QueryParameterFictionalUser? queryParameterFictionalUser,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single fictional user by id.
    ///     <see href="https://docs.bexio.com/#tag/Fictional-User-Management/operation/v3ShowFictionalUser">
    ///         Show Fictional User
    ///     </see>
    /// </summary>
    /// <param name="id">The id of the fictional user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the fictional user.</returns>
    public Task<ApiResult<FictionalUser>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a new fictional user.
    ///     <see href="https://docs.bexio.com/#tag/Fictional-User-Management/operation/v3CreateFictionalUser">
    ///         Create Fictional User
    ///     </see>
    /// </summary>
    /// <param name="fictionalUser">Create view describing the new fictional user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the created fictional user.</returns>
    public Task<ApiResult<FictionalUser>> Create(FictionalUserCreate fictionalUser,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Partially update an existing fictional user. Only the non-null fields on the patch view
    ///     are serialized and sent to Bexio; unspecified fields retain their current server-side value.
    ///     <see href="https://docs.bexio.com/#tag/Fictional-User-Management/operation/v3EditFictionalUser">
    ///         Edit Fictional User
    ///     </see>
    /// </summary>
    /// <param name="id">The id of the fictional user to update.</param>
    /// <param name="fictionalUser">Patch view describing the fields to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the updated fictional user.</returns>
    public Task<ApiResult<FictionalUser>> Patch(int id, FictionalUserPatch fictionalUser,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Permanently delete a fictional user by id. This cannot be undone.
    ///     <see href="https://docs.bexio.com/#tag/Fictional-User-Management/operation/v3DeleteFictionalUser">
    ///         Delete Fictional User
    ///     </see>
    /// </summary>
    /// <param name="id">The id of the fictional user to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> indicating the outcome of the delete.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
