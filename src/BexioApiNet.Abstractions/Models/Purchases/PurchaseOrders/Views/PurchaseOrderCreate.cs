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

namespace BexioApiNet.Abstractions.Models.Purchases.PurchaseOrders.Views;

/// <summary>
/// Create view for <c>POST /3.0/purchase/orders</c>. Read-only fields (id, totals,
/// updated_at) are intentionally omitted. <c>document_no</c> is optional — Bexio
/// auto-generates it when omitted.
/// <see href="https://docs.bexio.com/#tag/Purchase-Orders">Purchase Orders</see>
/// </summary>
/// <param name="ContactId">Identifier of the supplier contact the order is addressed to.</param>
/// <param name="CurrencyId">Identifier of the currency in which the order is denominated.</param>
/// <param name="UserId">Identifier of the user that owns the purchase order.</param>
/// <param name="DocumentNo">Optional document number. Auto-generated when omitted.</param>
/// <param name="Title">Free-form purchase order title.</param>
/// <param name="ContactSubId">Optional additional contact reference (e.g. addressee within the supplier company).</param>
/// <param name="IsValidFrom">Order validity start in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="IsValidUntil">Order validity end in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="ApiReference">Caller-supplied reference accessible only via the API.</param>
public sealed record PurchaseOrderCreate(
    [property: JsonPropertyName("contact_id")] int ContactId,
    [property: JsonPropertyName("currency_id")] int CurrencyId,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("document_no")] string? DocumentNo = null,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("contact_sub_id")] int? ContactSubId = null,
    [property: JsonPropertyName("is_valid_from")] string? IsValidFrom = null,
    [property: JsonPropertyName("is_valid_until")] string? IsValidUntil = null,
    [property: JsonPropertyName("api_reference")] string? ApiReference = null
);
