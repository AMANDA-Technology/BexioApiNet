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
    public Task<ApiResult<ManualEntryEntry>> Create(ManualEntryEntryCreate manualEntryEntry, [Optional] CancellationToken cancellationToken);

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
    /// <param name="queryParameterManualEntry">Query parameter specific for bank accounts</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ManualEntry>>> Get([Optional] QueryParameterManualEntry queryParameterManualEntry, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);
}
