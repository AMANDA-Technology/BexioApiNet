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
using BexioApiNet.Abstractions.Models.Files.Files.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Files;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;
using BexioFile = BexioApiNet.Abstractions.Models.Files.Files.File;
using FileUsage = BexioApiNet.Abstractions.Models.Files.Files.FileUsage;

namespace BexioApiNet.Services.Connectors.Files;

/// <inheritdoc cref="IFileService" />
public sealed class FileService : ConnectorService, IFileService
{
    /// <summary>
    ///     The api endpoint version
    /// </summary>
    private const string ApiVersion = FileConfiguration.ApiVersion;

    /// <summary>
    ///     The api request path
    /// </summary>
    private const string EndpointRoot = FileConfiguration.EndpointRoot;

    /// <inheritdoc />
    public FileService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<BexioFile>?>> Get([Optional] QueryParameterFile? queryParameterFile,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<BexioFile>?>($"{ApiVersion}/{EndpointRoot}",
            queryParameterFile?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null ||
            res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<BexioFile>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterFile?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<BexioFile>> GetById(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<BexioFile>($"{ApiVersion}/{EndpointRoot}/{id}", null,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<byte[]>> Download(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetBinaryAsync($"{ApiVersion}/{EndpointRoot}/{id}/download", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<byte[]>> Preview(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetBinaryAsync($"{ApiVersion}/{EndpointRoot}/{id}/preview", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<FileUsage>> Usage(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<FileUsage>($"{ApiVersion}/{EndpointRoot}/{id}/usage", null,
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<IReadOnlyList<BexioFile>>> Upload(List<FileInfo> files,
        [Optional] CancellationToken cancellationToken)
    {
        var filesInStream = files.Select(file =>
            new Tuple<MemoryStream, string>(new MemoryStream(File.ReadAllBytes(file.FullName)), file.Name)).ToList();
        return await ConnectionHandler.PostMultiPartFileAsync<IReadOnlyList<BexioFile>>(filesInStream,
            $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<IReadOnlyList<BexioFile>>> Upload(List<Tuple<MemoryStream, string>> files,
        [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostMultiPartFileAsync<IReadOnlyList<BexioFile>>(files,
            $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<BexioFile>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterFile? queryParameterFile, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<BexioFile>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/search",
            queryParameterFile?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<BexioFile>> Patch(int id, FilePatch payload,
        [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync<BexioFile, FilePatch>(payload, $"{ApiVersion}/{EndpointRoot}/{id}",
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}
