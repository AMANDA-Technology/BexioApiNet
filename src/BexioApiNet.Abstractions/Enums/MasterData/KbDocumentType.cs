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

namespace BexioApiNet.Abstractions.Enums.MasterData;

/// <summary>
/// Bexio document-type discriminator used as the <c>{kb_document_type}</c> URL segment in
/// document-scoped endpoints such as Comments (<c>/2.0/{kb_document_type}/{document_id}/comment</c>).
/// Use <see cref="KbDocumentTypeExtensions.ToBexioString"/> to convert to the snake_case
/// path segment expected by the Bexio API.
/// <see href="https://docs.bexio.com/#tag/Comments">Comments</see>
/// </summary>
public enum KbDocumentType
{
    /// <summary>Quote (offer) document type. Maps to the <c>kb_offer</c> path segment.</summary>
    Offer,

    /// <summary>Order document type. Maps to the <c>kb_order</c> path segment.</summary>
    Order,

    /// <summary>Invoice document type. Maps to the <c>kb_invoice</c> path segment.</summary>
    Invoice
}

/// <summary>
/// Extension methods for <see cref="KbDocumentType"/> that translate the typed enum to the
/// snake_case string Bexio expects in the URL.
/// </summary>
public static class KbDocumentTypeExtensions
{
    /// <summary>
    /// Returns the Bexio API path segment for the given <see cref="KbDocumentType"/>
    /// (e.g. <c>kb_invoice</c>).
    /// </summary>
    /// <param name="kbDocumentType">The document-type discriminator.</param>
    /// <returns>The snake_case path segment used in the URL.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not a defined enum member.</exception>
    public static string ToBexioString(this KbDocumentType kbDocumentType) => kbDocumentType switch
    {
        KbDocumentType.Offer => "kb_offer",
        KbDocumentType.Order => "kb_order",
        KbDocumentType.Invoice => "kb_invoice",
        _ => throw new ArgumentOutOfRangeException(nameof(kbDocumentType), kbDocumentType, "Unknown kb_document_type value.")
    };
}
