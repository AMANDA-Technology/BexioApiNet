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
using BexioApiNet.Abstractions.Models.Files.Files.Views;
using BexioApiNet.Models;
using BexioFile = BexioApiNet.Abstractions.Models.Files.Files.File;
using FileUsage = BexioApiNet.Abstractions.Models.Files.Files.FileUsage;

namespace BexioApiNet.Interfaces.Connectors.Files;

/// <summary>
///     Service for the Bexio files endpoints. <see href="https://docs.bexio.com/#tag/Files">Files</see>
/// </summary>
public interface IFileService
{
    /// <summary>
    ///     List files uploaded for the current company.
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3ReadFiles">List Files</see>
    /// </summary>
    /// <param name="queryParameterFile">Optional query parameters (offset/order_by/archived_state).</param>
    /// <param name="autoPage">
    ///     When <see langword="true" />, transparently pages through all remaining results via
    ///     <c>X-Total-Count</c>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> with the full page (or all pages) of files.</returns>
    public Task<ApiResult<List<BexioFile>?>> Get([Optional] QueryParameterFile? queryParameterFile,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single file by id. <see href="https://docs.bexio.com/#tag/Files/operation/v3ReadFile">Show File</see>
    /// </summary>
    /// <param name="id">The file id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file metadata as returned by Bexio.</returns>
    public Task<ApiResult<BexioFile>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Download the raw binary content of a file.
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3DownloadFile">Download File</see>
    /// </summary>
    /// <param name="id">The file id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> whose <c>Data</c> contains the file bytes on success.</returns>
    public Task<ApiResult<byte[]>> Download(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Download a preview rendering of a file.
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3PreviewFile">Preview File</see>
    /// </summary>
    /// <param name="id">The file id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> whose <c>Data</c> contains the preview bytes on success.</returns>
    public Task<ApiResult<byte[]>> Preview(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Show usage information for a file (which document it is attached to).
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3ShowFile">Show File Usage</see>
    /// </summary>
    /// <param name="id">The file id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file's usage information.</returns>
    public Task<ApiResult<FileUsage>> Usage(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Upload one or more files as a multipart/form-data payload.
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3CreateFile">Create File</see>
    /// </summary>
    /// <param name="files">The files to upload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created file metadata as returned by Bexio.</returns>
    public Task<ApiResult<IReadOnlyList<BexioFile>>> Upload(List<FileInfo> files,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Upload one or more files as a multipart/form-data payload using in-memory streams.
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3CreateFile">Create File</see>
    /// </summary>
    /// <param name="files">The files to upload, each paired with its filename.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created file metadata as returned by Bexio.</returns>
    public Task<ApiResult<IReadOnlyList<BexioFile>>> Upload(List<Tuple<MemoryStream, string>> files,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search files by criteria. <see href="https://docs.bexio.com/#tag/Files/operation/v3SearchFile">Search Files</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterFile">Optional query parameters (limit/offset/archived_state).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching files.</returns>
    public Task<ApiResult<List<BexioFile>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterFile? queryParameterFile, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update an existing file's metadata.
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3UpdateFile">Update File</see>
    /// </summary>
    /// <param name="id">The file id.</param>
    /// <param name="payload">The file patch payload; only supplied fields are updated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated file metadata as returned by Bexio.</returns>
    public Task<ApiResult<BexioFile>> Patch(int id, FilePatch payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a file. Sets the file state to deleted; cannot be undone.
    ///     <see href="https://docs.bexio.com/#tag/Files/operation/v3DeleteFile">Delete File</see>
    /// </summary>
    /// <param name="id">The file id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
