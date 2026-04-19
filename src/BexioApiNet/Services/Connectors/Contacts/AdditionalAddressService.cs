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
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses;
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Contacts;

/// <inheritdoc cref="BexioApiNet.Interfaces.Connectors.Contacts.IAdditionalAddressService" />
public sealed class AdditionalAddressService : ConnectorService, IAdditionalAddressService
{
    /// <summary>
    /// The api endpoint version.
    /// </summary>
    private const string ApiVersion = AdditionalAddressConfiguration.ApiVersion;

    /// <summary>
    /// Parent resource segment (contacts) under which additional addresses are nested.
    /// </summary>
    private const string ParentEndpoint = AdditionalAddressConfiguration.ParentEndpoint;

    /// <summary>
    /// The trailing request path segment below the parent contact id.
    /// </summary>
    private const string EndpointSegment = AdditionalAddressConfiguration.EndpointSegment;

    /// <inheritdoc />
    public AdditionalAddressService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<AdditionalAddress>?>> Get(int contactId, [Optional] QueryParameterAdditionalAddress? queryParameterAdditionalAddress, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var endpointRoot = BuildEndpointRoot(contactId);
        var res = await ConnectionHandler.GetAsync<List<AdditionalAddress>?>(endpointRoot, queryParameterAdditionalAddress?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null || res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<AdditionalAddress>(
            res.Data.Count,
            totalResults,
            endpointRoot,
            queryParameterAdditionalAddress?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<AdditionalAddress>> GetById(int contactId, int id, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.GetAsync<AdditionalAddress>($"{BuildEndpointRoot(contactId)}/{id}", null, cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<AdditionalAddress>> Create(int contactId, AdditionalAddressCreate additionalAddress, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.PostAsync<AdditionalAddress, AdditionalAddressCreate>(additionalAddress, BuildEndpointRoot(contactId), cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<List<AdditionalAddress>>> Search(int contactId, List<SearchCriteria> searchCriteria, [Optional] QueryParameterAdditionalAddress? queryParameterAdditionalAddress, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.PostSearchAsync<AdditionalAddress>(searchCriteria, $"{BuildEndpointRoot(contactId)}/search", queryParameterAdditionalAddress?.QueryParameter, cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<AdditionalAddress>> Update(int contactId, int id, AdditionalAddressUpdate additionalAddress, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.PostAsync<AdditionalAddress, AdditionalAddressUpdate>(additionalAddress, $"{BuildEndpointRoot(contactId)}/{id}", cancellationToken);

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int contactId, int id, [Optional] CancellationToken cancellationToken)
        => await ConnectionHandler.Delete($"{BuildEndpointRoot(contactId)}/{id}", cancellationToken);

    private static string BuildEndpointRoot(int contactId) => $"{ApiVersion}/{ParentEndpoint}/{contactId}/{EndpointSegment}";
}
