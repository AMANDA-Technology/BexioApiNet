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

namespace BexioApiNet.Abstractions.Enums.Sales;

/// <summary>
/// Bexio document-type path segments used as the <c>{kb_document_type}</c> parameter in
/// document-position endpoints (e.g. <c>/2.0/{kb_document_type}/{document_id}/kb_position_*</c>).
/// <see href="https://docs.bexio.com/#tag/Sub-positions"/>
/// </summary>
public static class KbDocumentType
{
    /// <summary>Invoice document type. Maps to the <c>kb_invoice</c> path segment.</summary>
    public const string Invoice = "kb_invoice";

    /// <summary>Quote (offer) document type. Maps to the <c>kb_offer</c> path segment.</summary>
    public const string Offer = "kb_offer";

    /// <summary>Order document type. Maps to the <c>kb_order</c> path segment.</summary>
    public const string Order = "kb_order";
}
