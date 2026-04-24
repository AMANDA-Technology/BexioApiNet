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
using BexioApiNet.Abstractions.Models.MasterData.Salutations;
using BexioApiNet.Abstractions.Models.MasterData.Salutations.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
///     Service for managing salutations. <see href="https://docs.bexio.com/#tag/Salutations">Salutations</see>
/// </summary>
public interface ISalutationService
{
    /// <summary>
    ///     Fetch a list of salutations.
    ///     <see href="https://docs.bexio.com/#tag/Salutations/operation/v2ListSalutations">List Salutations</see>
    /// </summary>
    /// <param name="queryParameterSalutation">Query parameter specific for salutation</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Salutation>?>> Get([Optional] QueryParameterSalutation? queryParameterSalutation,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single salutation by id.
    ///     <see href="https://docs.bexio.com/#tag/Salutations/operation/v2ShowSalutation">Show Salutation</see>
    /// </summary>
    /// <param name="id">The salutation id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Salutation?>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a salutation.
    ///     <see href="https://docs.bexio.com/#tag/Salutations/operation/v2CreateSalutation">Create Salutation</see>
    /// </summary>
    /// <param name="salutation">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Salutation>> Create(SalutationCreate salutation,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search salutations.
    ///     <see href="https://docs.bexio.com/#tag/Salutations/operation/v2SearchSalutations">Search Salutations</see>
    /// </summary>
    /// <param name="searchCriteria">The search criteria list. Supported fields: <c>name</c>.</param>
    /// <param name="queryParameterSalutation">Optional pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Salutation>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterSalutation? queryParameterSalutation, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update (edit) a salutation.
    ///     <see href="https://docs.bexio.com/#tag/Salutations/operation/v2EditSalutation">Edit Salutation</see>
    /// </summary>
    /// <param name="id">The salutation id</param>
    /// <param name="salutation">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Salutation>> Update(int id, SalutationUpdate salutation,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a salutation.
    ///     <see href="https://docs.bexio.com/#tag/Salutations/operation/v2DeleteSalutation">Delete Salutation</see>
    /// </summary>
    /// <param name="id">The salutation id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}