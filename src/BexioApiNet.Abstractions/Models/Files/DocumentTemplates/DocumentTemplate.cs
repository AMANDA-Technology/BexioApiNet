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

using BexioApiNet.Abstractions.Models.Files.DocumentTemplates.Enums;

namespace BexioApiNet.Abstractions.Models.Files.DocumentTemplates;

/// <summary>
/// Document template returned by the Bexio v3.0 <c>/document_templates</c> endpoint.
/// Referenced by sales documents via the <c>template_slug</c> field.
/// <see href="https://docs.bexio.com/#tag/Document-templates/operation/v3ListDocumentTemplate">v3 List Document Templates</see>
/// </summary>
/// <param name="TemplateSlug">Document template identifier (a.k.a. slug).</param>
/// <param name="Name">Document template display name.</param>
/// <param name="IsDefault">Whether this template is the default for at least one document type.</param>
/// <param name="DefaultForDocumentTypes">Document types for which this template is the default.</param>
public sealed record DocumentTemplate(
    [property: JsonPropertyName("template_slug")] string TemplateSlug,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("is_default")] bool IsDefault,
    [property: JsonPropertyName("default_for_document_types")] IReadOnlyList<DocumentTemplateDocumentType> DefaultForDocumentTypes
);
