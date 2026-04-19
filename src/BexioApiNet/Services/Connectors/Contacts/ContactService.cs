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
using BexioApiNet.Abstractions.Models.Contacts.Contacts;
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Contacts;

/// <inheritdoc cref="IContactService" />
public sealed class ContactService : ConnectorService, IContactService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = ContactConfiguration.ApiVersion;

    /// <summary>
    /// The api request path
    /// </summary>
    private const string EndpointRoot = ContactConfiguration.EndpointRoot;

    /// <inheritdoc />
    public ContactService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Contact>?>> Get([Optional] QueryParameterContact? queryParameterContact, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<Contact>?>($"{ApiVersion}/{EndpointRoot}", queryParameterContact?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null || res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<Contact>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterContact?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<Contact>> GetById(int id, [Optional] QueryParameterContact? queryParameterContact, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Contact>($"{ApiVersion}/{EndpointRoot}/{id}", queryParameterContact?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Contact>> Create(ContactCreate contact, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Contact, ContactCreate>(contact, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Contact>>> BulkCreate(List<ContactCreate> contacts, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostBulkAsync<Contact, ContactCreate>(contacts, $"{ApiVersion}/{EndpointRoot}/_bulk_create", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Contact>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterContact? queryParameterContact, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<Contact>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/search", queryParameterContact?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Contact>> Update(int id, ContactUpdate contact, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Contact, ContactUpdate>(contact, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Restore(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync<object, object?>(null, $"{ApiVersion}/{EndpointRoot}/{id}/restore", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}
