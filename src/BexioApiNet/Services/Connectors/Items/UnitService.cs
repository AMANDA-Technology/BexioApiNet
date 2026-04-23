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
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Items.Units;
using BexioApiNet.Abstractions.Models.Items.Units.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Items;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Items;

/// <inheritdoc cref="BexioApiNet.Interfaces.Connectors.Items.IUnitService" />
public sealed class UnitService : ConnectorService, IUnitService
{
    /// <summary>
    ///     The api endpoint version
    /// </summary>
    private const string ApiVersion = UnitConfiguration.ApiVersion;

    /// <summary>
    ///     The api request path
    /// </summary>
    private const string EndpointRoot = UnitConfiguration.EndpointRoot;

    /// <inheritdoc />
    public UnitService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Unit>?>> Get([Optional] QueryParameterUnit? queryParameterUnit,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<Unit>?>($"{ApiVersion}/{EndpointRoot}",
            queryParameterUnit?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null ||
            res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<Unit>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterUnit?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<Unit?>> GetById(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Unit?>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Unit>> Create(UnitCreate unit, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Unit, UnitCreate>(unit, $"{ApiVersion}/{EndpointRoot}",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Unit>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterUnit? queryParameterUnit, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<Unit>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/search",
            queryParameterUnit?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Unit>> Update(int id, UnitUpdate unit, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PutAsync<Unit, UnitUpdate>(unit, $"{ApiVersion}/{EndpointRoot}/{id}",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}