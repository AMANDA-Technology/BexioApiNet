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

using BexioApiNet.Abstractions.Models.Sales.Positions;

namespace BexioApiNet.Abstractions.Models.Sales.Orders.Views;

/// <summary>
/// Body for <c>POST /2.0/kb_order/{order_id}/delivery</c> and <c>POST /2.0/kb_order/{order_id}/invoice</c>.
/// When <see cref="Positions" /> is <see langword="null" />, Bexio copies every position from the source
/// order; otherwise the supplied subset is used. Positions are typed as the polymorphic
/// <see cref="Position" /> union so callers can build the desired subtype strongly.
/// <see href="https://docs.bexio.com/#tag/Orders/operation/v2CreateDeliveryFromOrder" />
/// <see href="https://docs.bexio.com/#tag/Orders/operation/v2CreateInvoiceFromOrder" />
/// </summary>
/// <param name="Positions">Optional subset of positions to carry over to the new document. Omit to copy all.</param>
public sealed record OrderConvertRequest(
    [property: JsonPropertyName("positions")]
    IReadOnlyList<Position>? Positions = null
);
