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

// ReSharper disable InconsistentNaming
namespace BexioApiNet.Abstractions.Models.Files.DocumentTemplates.Enums;

/// <summary>
/// Document types a <see cref="DocumentTemplate" /> may be marked as default for. The
/// serialized form matches the Bexio <c>default_for_document_types</c> enum values exactly.
/// <see href="https://docs.bexio.com/#tag/Document-templates/operation/v3ListDocumentTemplate">v3 List Document Templates</see>
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DocumentTemplateDocumentType
{
    /// <summary>Quote (offer) document type.</summary>
    type_offer,

    /// <summary>Order document type.</summary>
    type_order,

    /// <summary>Invoice document type.</summary>
    type_invoice,

    /// <summary>Delivery document type.</summary>
    type_delivery,

    /// <summary>Credit voucher document type.</summary>
    type_credit_voucher,

    /// <summary>Account statement document type.</summary>
    type_account_statement,

    /// <summary>Article order document type.</summary>
    type_article_order
}
