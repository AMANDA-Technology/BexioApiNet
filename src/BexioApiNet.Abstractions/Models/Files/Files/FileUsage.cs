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
///     File usage entity returned by the Bexio v3.0 <c>GET /3.0/files/{file_id}/usage</c> endpoint.
///     Describes which document a file is attached to.
///     <see href="https://docs.bexio.com/#tag/Files/operation/v3ShowFile">Show File usage</see>
/// </summary>
/// <param name="Id">The id of the file.</param>
/// <param name="RefClass">The reference to the class this file is attached to (e.g. <c>KbInvoice</c>).</param>
/// <param name="Title">The title set on the reference class.</param>
/// <param name="DocumentNr">The internal document number set on the reference class.</param>
public sealed record FileUsage(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("ref_class")]
    string RefClass,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("document_nr")]
    string DocumentNr
);
