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

namespace BexioApiNet.Abstractions.Models.Files.Files;

/// <summary>
///     File entity returned by the Bexio v3.0 <c>/files</c> endpoints (list, show, upload, patch).
///     <see href="https://docs.bexio.com/#tag/Files">Files</see>
/// </summary>
/// <param name="Id">The id of the file.</param>
/// <param name="Uuid">The uuid of the file.</param>
/// <param name="Name">The name of the file.</param>
/// <param name="SizeInBytes">The size of the file in bytes.</param>
/// <param name="Extension">The extension of the file.</param>
/// <param name="MimeType">The mime type of the file.</param>
/// <param name="UploaderEmail">Returns the email of the sender if the file was added by email.</param>
/// <param name="UserId">The id of the user which originally uploaded the file (references a user object).</param>
/// <param name="IsArchived">Whether the file is archived.</param>
/// <param name="SourceId">
///     ID of the source (web, mobile, etc.) this file has been uploaded from. Deprecated by Bexio —
///     prefer <see cref="SourceType" />.
/// </param>
/// <param name="SourceType">Type of the source (<c>web</c>, <c>email</c>, <c>mobile</c>) this file has been uploaded from.</param>
/// <param name="IsReferenced">Whether the file is referenced to a document or not.</param>
/// <param name="CreatedAt">File upload date.</param>
public sealed record File(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("uuid")] Guid Uuid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("size_in_bytes")]
    long SizeInBytes,
    [property: JsonPropertyName("extension")]
    string Extension,
    [property: JsonPropertyName("mime_type")]
    string MimeType,
    [property: JsonPropertyName("uploader_email")]
    string? UploaderEmail,
    [property: JsonPropertyName("user_id")]
    int UserId,
    [property: JsonPropertyName("is_archived")]
    bool IsArchived,
    [property: JsonPropertyName("source_id")]
    int? SourceId,
    [property: JsonPropertyName("source_type")]
    string? SourceType,
    [property: JsonPropertyName("is_referenced")]
    bool IsReferenced,
    [property: JsonPropertyName("created_at")]
    DateTime CreatedAt
);
