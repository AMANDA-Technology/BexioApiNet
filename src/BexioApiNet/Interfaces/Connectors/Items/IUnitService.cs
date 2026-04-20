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
using BexioApiNet.Abstractions.Models.Items.Units;
using BexioApiNet.Abstractions.Models.Items.Units.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Items;

/// <summary>
///     Service for managing units. <see href="https://docs.bexio.com/#tag/Units">Units</see>
/// </summary>
public interface IUnitService
{
    /// <summary>
    ///     Fetch a list of units. <see href="https://docs.bexio.com/#tag/Units/operation/v2ListUnits">List Units</see>
    /// </summary>
    /// <param name="queryParameterUnit">Query parameter specific for unit</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Unit>?>> Get([Optional] QueryParameterUnit? queryParameterUnit, [Optional] bool autoPage,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single unit by id. <see href="https://docs.bexio.com/#tag/Units/operation/v2ShowUnit">Show Unit</see>
    /// </summary>
    /// <param name="id">The unit id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Unit?>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a unit. <see href="https://docs.bexio.com/#tag/Units/operation/v2CreateUnit">Create Unit</see>
    /// </summary>
    /// <param name="unit">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Unit>> Create(UnitCreate unit, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search units. <see href="https://docs.bexio.com/#tag/Units/operation/v2SearchUnits">Search Units</see>
    /// </summary>
    /// <param name="searchCriteria">The search criteria list. Supported fields: <c>name</c>.</param>
    /// <param name="queryParameterUnit">Optional pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Unit>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterUnit? queryParameterUnit, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update (edit) a unit. <see href="https://docs.bexio.com/#tag/Units/operation/v2EditUnit">Edit Unit</see>
    /// </summary>
    /// <param name="id">The unit id</param>
    /// <param name="unit">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Unit>> Update(int id, UnitUpdate unit, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a unit. <see href="https://docs.bexio.com/#tag/Units/operation/v2DeleteUnit">Delete Unit</see>
    /// </summary>
    /// <param name="id">The unit id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}