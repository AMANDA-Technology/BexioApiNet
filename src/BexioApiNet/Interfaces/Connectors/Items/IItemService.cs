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
using BexioApiNet.Abstractions.Models.Items.Items;
using BexioApiNet.Abstractions.Models.Items.Items.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Items;

/// <summary>
///     Service for the Bexio items endpoints. <see href="https://docs.bexio.com/#tag/Items">Items</see>
/// </summary>
public interface IItemService
{
    /// <summary>
    ///     List all items. <see href="https://docs.bexio.com/#tag/Items/operation/v2ListItems">List Items</see>
    /// </summary>
    /// <param name="queryParameterItem">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">
    ///     When <see langword="true" />, transparently pages through all remaining results via
    ///     <c>X-Total-Count</c>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> with the full page (or all pages) of items.</returns>
    public Task<ApiResult<List<Item>?>> Get([Optional] QueryParameterItem? queryParameterItem, [Optional] bool autoPage,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single item by id. <see href="https://docs.bexio.com/#tag/Items/operation/v2ShowItem">Show Item</see>
    /// </summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The item matching the given id.</returns>
    public Task<ApiResult<Item>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a single item. <see href="https://docs.bexio.com/#tag/Items/operation/v2CreateItem">Create Item</see>
    /// </summary>
    /// <param name="item">The item create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created item as returned by Bexio.</returns>
    public Task<ApiResult<Item>> Create(ItemCreate item, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search items by criteria. <see href="https://docs.bexio.com/#tag/Items/operation/v2SearchItems">Search Items</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterItem">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching items.</returns>
    public Task<ApiResult<List<Item>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterItem? queryParameterItem, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Edit an existing item. Bexio uses <c>POST /2.0/article/{id}</c> (not <c>PUT</c>) for
    ///     full-replacement updates — see <see href="https://docs.bexio.com/#tag/Items/operation/v2EditItem">Edit Item</see>.
    /// </summary>
    /// <param name="id">The item id.</param>
    /// <param name="item">The item update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated item as returned by Bexio.</returns>
    public Task<ApiResult<Item>> Update(int id, ItemUpdate item, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete an item. <see href="https://docs.bexio.com/#tag/Items/operation/DeleteItem">Delete Item</see>
    /// </summary>
    /// <param name="id">The item id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}