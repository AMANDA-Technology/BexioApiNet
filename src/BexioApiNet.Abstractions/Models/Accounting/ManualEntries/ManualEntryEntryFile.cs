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

using BexioApiNet.Abstractions.Models.Accounting.ManualEntries.Enums;

namespace BexioApiNet.Abstractions.Models.Accounting.ManualEntries;

/// <summary>
/// File metadata returned by the Bexio manual entry line file upload endpoint. Matches the
/// shape of <c>FileResponse</c> in the Bexio v3 OpenAPI spec.
/// <see href="https://docs.bexio.com/#tag/Manual-Entries/operation/UploadManualEntryFile">Upload Manual Entry File</see>
/// </summary>
/// <param name="Id">The id of the uploaded file.</param>
/// <param name="Uuid">The uuid of the file.</param>
/// <param name="Name">The display name of the file. Maximum 80 characters.</param>
/// <param name="SizeInBytes">The file size in bytes.</param>
/// <param name="Extension">The file extension. Maximum 10 characters.</param>
/// <param name="MimeType">The mime type of the file. Maximum 80 characters.</param>
/// <param name="UploaderEmail">Email of the uploader. Nullable.</param>
/// <param name="UserId">The id of the user who uploaded the file.</param>
/// <param name="IsArchived">Indicates whether the file is archived.</param>
/// <param name="SourceType">The source where the file was uploaded from (web, email, mobile). Nullable.</param>
/// <param name="IsReferenced">Indicates whether the file is referenced.</param>
/// <param name="CreatedAt">Timestamp when the file was created.</param>
public sealed record ManualEntryEntryFile(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("uuid")] string Uuid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("size_in_bytes")] long SizeInBytes,
    [property: JsonPropertyName("extension")] string Extension,
    [property: JsonPropertyName("mime_type")] string MimeType,
    [property: JsonPropertyName("uploader_email")] string? UploaderEmail,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("is_archived")] bool IsArchived,
    [property: JsonPropertyName("source_type")] ManualEntryFileSourceType? SourceType,
    [property: JsonPropertyName("is_referenced")] bool IsReferenced,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt
);
