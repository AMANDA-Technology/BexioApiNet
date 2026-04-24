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
using BexioApiNet.Abstractions.Models.MasterData.Notes;
using BexioApiNet.Abstractions.Models.MasterData.Notes.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
///     Service for managing notes. <see href="https://docs.bexio.com/#tag/Notes">Notes</see>
/// </summary>
public interface INoteService
{
    /// <summary>
    ///     Fetch a list of notes. <see href="https://docs.bexio.com/#tag/Notes/operation/v2ListNotes">List Notes</see>
    /// </summary>
    /// <param name="queryParameterNote">Query parameter specific for note</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Note>?>> Get([Optional] QueryParameterNote? queryParameterNote, [Optional] bool autoPage,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single note by id. <see href="https://docs.bexio.com/#tag/Notes/operation/v2ShowNote">Show Note</see>
    /// </summary>
    /// <param name="id">The note id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Note?>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a note. <see href="https://docs.bexio.com/#tag/Notes/operation/v2CreateNote">Create Note</see>
    /// </summary>
    /// <param name="note">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Note>> Create(NoteCreate note, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search notes. <see href="https://docs.bexio.com/#tag/Notes/operation/v2SearchNotes">Search Notes</see>
    /// </summary>
    /// <param name="searchCriteria">
    ///     The search criteria list. Supported fields: <c>event_start</c>, <c>contact_id</c>,
    ///     <c>user_id</c>, <c>subject</c>, <c>module_id</c>, <c>entry_id</c>.
    /// </param>
    /// <param name="queryParameterNote">Optional pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Note>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterNote? queryParameterNote, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update (edit) a note. <see href="https://docs.bexio.com/#tag/Notes/operation/v2EditNote">Edit Note</see>
    /// </summary>
    /// <param name="id">The note id</param>
    /// <param name="note">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Note>> Update(int id, NoteUpdate note, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a note. <see href="https://docs.bexio.com/#tag/Notes/operation/DeleteNote">Delete Note</see>
    /// </summary>
    /// <param name="id">The note id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}