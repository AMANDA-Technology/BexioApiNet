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
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries;
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Accounting;

/// <inheritdoc cref="BexioApiNet.Interfaces.Connectors.Accounting.IManualEntryService" />
public sealed class ManualEntryService : ConnectorService, IManualEntryService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = ManualEntryConfiguration.ApiVersion;

    /// <summary>
    /// The api request path
    /// </summary>
    private const string EndpointRoot = ManualEntryConfiguration.EndpointRoot;

    /// <inheritdoc />
    public ManualEntryService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<ManualEntry>> Create(ManualEntryEntryCreate manualEntryEntry, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<ManualEntry, ManualEntryEntryCreate>(manualEntryEntry, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<IReadOnlyList<ManualEntryEntryFile>>> AddAttachment(int manuelEntriesId, int manuelEntryId, List<FileInfo> files, [Optional] CancellationToken cancellationToken)
    {
        var filesInStream = files.Select(file => new Tuple<MemoryStream, string>(new(File.ReadAllBytes(file.FullName)), file.Name)).ToList();
        return await ConnectionHandler.PostMultiPartFileAsync<IReadOnlyList<ManualEntryEntryFile>>(filesInStream, $"{ApiVersion}/{EndpointRoot}/{manuelEntriesId}/entries/{manuelEntryId}/files", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<IReadOnlyList<ManualEntryEntryFile>>> AddAttachment(int manuelEntriesId, int manuelEntryId, List<Tuple<MemoryStream, string>> files, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostMultiPartFileAsync<IReadOnlyList<ManualEntryEntryFile>>(files, $"{ApiVersion}/{EndpointRoot}/{manuelEntriesId}/entries/{manuelEntryId}/files", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<ManualEntry>?>> Get([Optional] QueryParameterManualEntry? queryParameterManualEntry, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<ManualEntry>?>($"{ApiVersion}/{EndpointRoot}", queryParameterManualEntry?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null || res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<ManualEntry>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterManualEntry?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
        return res;
    }
}
