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
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries;
using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Accounting;

/// <summary>
/// Service for creating a manual entry in accounting namespace. <see href="https://docs.bexio.com/#tag/Manual-Entries">Manual Entries</see>
/// </summary>
public interface IManualEntryService
{
    /// <summary>
    /// Create a manual entry. <see href="https://docs.bexio.com/#tag/Manual-Entries">Manual Entries</see>
    /// </summary>
    /// <param name="manualEntryEntry">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ManualEntry>> Create(ManualEntryEntryCreate manualEntryEntry, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update a manual entry. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/UpdateManualEntry">Update Manual Entry</see>
    /// </summary>
    /// <param name="manualEntryId">The manual entry id</param>
    /// <param name="payload">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ManualEntry>> Put(int manualEntryId, ManualEntryUpdate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Add one or more attachments to a manual entry. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/UploadManualEntryFile">Upload Manual Entry File</see>
    /// </summary>
    /// <param name="manuelEntriesId">The manual entry root id</param>
    /// <param name="manuelEntryId">The manual root entry entry id</param>
    /// <param name="files">List with files to attach</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<IReadOnlyList<ManualEntryEntryFile>>> AddAttachment(int manuelEntriesId, int manuelEntryId, List<FileInfo> files, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Add one or more attachments to a manual entry. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/UploadManualEntryFile">Upload Manual Entry File</see>
    /// </summary>
    /// <param name="manuelEntriesId">The manual entry root id</param>
    /// <param name="manuelEntryId">The manual root entry entry id</param>
    /// <param name="files">List with files to attach</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<IReadOnlyList<ManualEntryEntryFile>>> AddAttachment(int manuelEntriesId, int manuelEntryId, List<Tuple<MemoryStream, string>> files, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a list of manual entries. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/ListManualEntries">List Manual Entries</see>
    /// </summary>
    /// <param name="queryParameterManualEntry">Query parameter specific for manual entry</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ManualEntry>?>> Get([Optional] QueryParameterManualEntry? queryParameterManualEntry, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get the suggested next reference number for a manual entry. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/GetNextReferenceNumber">Get Next Reference Number</see>
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ManualEntryNextRefNr>> GetNextRefNr([Optional] CancellationToken cancellationToken);

    /// <summary>
    /// List all files attached to a manual compound entry. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/ListManualCompoundEntryFiles">List Manual Compound Entry Files</see>
    /// </summary>
    /// <param name="manualEntryId">The manual entry id</param>
    /// <param name="queryParameter">Query parameter (limit, offset)</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ManualEntryFile>?>> GetFiles(int manualEntryId, [Optional] QueryParameterManualEntry? queryParameter, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single file attached to a manual compound entry, including its base64-encoded content.
    /// <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/ShowManualCompoundEntryFile">Show Manual Compound Entry File</see>
    /// </summary>
    /// <param name="manualEntryId">The manual entry id</param>
    /// <param name="fileId">The file id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ManualEntryFileDetail>> GetFileById(int manualEntryId, int fileId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// List all files attached to a specific manual entry line. <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/ListManualEntryFiles">List Manual Entry Files</see>
    /// </summary>
    /// <param name="manualEntryId">The manual entry id</param>
    /// <param name="entryId">The manual entry line id</param>
    /// <param name="queryParameter">Query parameter (limit, offset)</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ManualEntryFile>?>> GetEntryFiles(int manualEntryId, int entryId, [Optional] QueryParameterManualEntry? queryParameter, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single file attached to a manual entry line, including its base64-encoded content.
    /// <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/ShowManualEntryFile">Show Manual Entry File</see>
    /// </summary>
    /// <param name="manualEntryId">The manual entry id</param>
    /// <param name="entryId">The manual entry line id</param>
    /// <param name="fileId">The file id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ManualEntryFileDetail>> GetEntryFileById(int manualEntryId, int entryId, int fileId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a manual entry
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete the connection between a file and a manual compound entry.
    /// <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/DeleteManualCompoundEntryFile">Delete Manual Compound Entry File</see>
    /// </summary>
    /// <param name="manualEntryId">The manual entry id</param>
    /// <param name="fileId">The file id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> DeleteFile(int manualEntryId, int fileId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete the connection between a file and a manual entry line.
    /// <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/DeleteManualEntryFile">Delete Manual Entry File</see>
    /// </summary>
    /// <param name="manualEntryId">The manual entry id</param>
    /// <param name="entryId">The manual entry line id</param>
    /// <param name="fileId">The file id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> DeleteEntryFile(int manualEntryId, int entryId, int fileId, [Optional] CancellationToken cancellationToken);
}
