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
using BexioApiNet.Abstractions.Models.MasterData.Users;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
///     Service for reading Bexio user accounts.
///     <see href="https://docs.bexio.com/#tag/User-Management">User Management</see>
/// </summary>
public interface IUserService
{
    /// <summary>
    ///     Get a list of users.
    ///     <see href="https://docs.bexio.com/#tag/User-Management/operation/v3ListUsers">List Users</see>
    /// </summary>
    /// <param name="queryParameterUser">Optional pagination query parameters.</param>
    /// <param name="autoPage">Fetch all remaining pages automatically when <c>true</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the list of users.</returns>
    public Task<ApiResult<List<User>>> GetAll([Optional] QueryParameterUser? queryParameterUser,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch the authenticated user (singleton <c>/users/me</c> endpoint).
    ///     <see href="https://docs.bexio.com/#tag/User-Management/operation/v3ShowLoggedInUser">Show Logged-in User</see>
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the logged-in user.</returns>
    public Task<ApiResult<User>> GetMe([Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single user by id.
    ///     <see href="https://docs.bexio.com/#tag/User-Management/operation/v3ShowUser">Show User</see>
    /// </summary>
    /// <param name="id">The id of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping the user.</returns>
    public Task<ApiResult<User>> GetById(int id, [Optional] CancellationToken cancellationToken);
}
