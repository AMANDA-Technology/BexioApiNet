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
using BexioApiNet.Abstractions.Models.MasterData.Titles;
using BexioApiNet.Abstractions.Models.MasterData.Titles.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
///     Service for managing titles. <see href="https://docs.bexio.com/#tag/Titles">Titles</see>
/// </summary>
public interface ITitleService
{
    /// <summary>
    ///     Fetch a list of titles. <see href="https://docs.bexio.com/#tag/Titles/operation/v2ListTitles">List Titles</see>
    /// </summary>
    /// <param name="queryParameterTitle">Query parameter specific for title</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Title>?>> Get([Optional] QueryParameterTitle? queryParameterTitle,
        [Optional] bool autoPage,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single title by id. <see href="https://docs.bexio.com/#tag/Titles/operation/v2ShowTitle">Show Title</see>
    /// </summary>
    /// <param name="id">The title id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Title?>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a title. <see href="https://docs.bexio.com/#tag/Titles/operation/v2CreateTitle">Create Title</see>
    /// </summary>
    /// <param name="title">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Title>> Create(TitleCreate title, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search titles. <see href="https://docs.bexio.com/#tag/Titles/operation/v2SearchTitles">Search Titles</see>
    /// </summary>
    /// <param name="searchCriteria">The search criteria list. Supported fields: <c>name</c>.</param>
    /// <param name="queryParameterTitle">Optional pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Title>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterTitle? queryParameterTitle, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update (edit) a title. <see href="https://docs.bexio.com/#tag/Titles/operation/v2EditTitle">Edit Title</see>
    /// </summary>
    /// <param name="id">The title id</param>
    /// <param name="title">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Title>> Update(int id, TitleUpdate title, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a title. <see href="https://docs.bexio.com/#tag/Titles/operation/v2DeleteTitle">Delete Title</see>
    /// </summary>
    /// <param name="id">The title id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}