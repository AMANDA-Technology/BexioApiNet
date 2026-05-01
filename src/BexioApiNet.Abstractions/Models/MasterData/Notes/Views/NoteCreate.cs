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

using BexioApiNet.Abstractions.Json;

namespace BexioApiNet.Abstractions.Models.MasterData.Notes.Views;

/// <summary>
///     Create view for a note — body of <c>POST /2.0/note</c>. The read-only <c>id</c> and
///     <c>project_id</c> fields are intentionally omitted; use <see cref="PrProjectId" /> to
///     link the new note to a project (Bexio mirrors the value onto <c>project_id</c> on reads).
///     <see href="https://docs.bexio.com/#tag/Notes/operation/v2CreateNote" />
/// </summary>
/// <param name="UserId">Reference to the user that owns the note.</param>
/// <param name="EventStart">Timestamp of the note's event in Bexio's <c>yyyy-MM-dd HH:mm:ss</c> format.</param>
/// <param name="Subject">Short subject / title of the note.</param>
/// <param name="Info">
///     Free-text body of the note. Optional; defaults to <see langword="null" /> and is only serialized
///     when supplied.
/// </param>
/// <param name="ContactId">Optional reference to a linked contact object.</param>
/// <param name="PrProjectId">Optional reference to a project object (write-only; surfaced as <c>project_id</c> on reads).</param>
/// <param name="EntryId">Optional reference to the linked document entry.</param>
/// <param name="ModuleId">Optional reference to the module that owns <see cref="EntryId" />.</param>
public sealed record NoteCreate(
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("event_start")]
    [property: JsonConverter(typeof(BexioDateTimeJsonConverter))]
    DateTime EventStart,
    [property: JsonPropertyName("subject")]
    string Subject,
    [property: JsonPropertyName("info")] string? Info = null,
    [property: JsonPropertyName("contact_id")]
    int? ContactId = null,
    [property: JsonPropertyName("pr_project_id")]
    int? PrProjectId = null,
    [property: JsonPropertyName("entry_id")]
    int? EntryId = null,
    [property: JsonPropertyName("module_id")]
    int? ModuleId = null
);