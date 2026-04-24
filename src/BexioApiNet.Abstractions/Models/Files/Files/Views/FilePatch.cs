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

namespace BexioApiNet.Abstractions.Models.Files.Files.Views;

/// <summary>
///     Patch view for <c>PATCH /3.0/files/{file_id}</c>. All properties are optional — only the
///     fields supplied are sent to the server. <see langword="null" /> values are omitted from the
///     serialized payload so unset fields are not mutated.
///     <see href="https://docs.bexio.com/#tag/Files/operation/v3UpdateFile">Update File</see>
/// </summary>
/// <param name="Name">The new name of the file (max. 255 characters).</param>
/// <param name="IsArchived">Define archived state of the file.</param>
/// <param name="SourceType">Type of the source (<c>web</c>, <c>email</c>, <c>mobile</c>) this file has been uploaded from.</param>
public sealed record FilePatch(
    [property: JsonPropertyName("name")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Name = null,
    [property: JsonPropertyName("is_archived")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? IsArchived = null,
    [property: JsonPropertyName("source_type")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? SourceType = null
);
