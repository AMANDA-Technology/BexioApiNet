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

using BexioApiNet.Abstractions.Models.Purchases.Bills.Enums;

namespace BexioApiNet.Abstractions.Models.Purchases.Bills;

/// <summary>
/// Address block attached to a bill. The same shape is used on the create/update
/// payloads and on the responses of the Bexio v4.0 <c>/purchase/bills</c> endpoints.
/// <see href="https://docs.bexio.com/#tag/Bills">Bills</see>
/// </summary>
/// <param name="LastnameCompany">Family name (for <c>PRIVATE</c>) or company name (for <c>COMPANY</c>). Required.</param>
/// <param name="Type">Address contact type (<c>PRIVATE</c> or <c>COMPANY</c>).</param>
/// <param name="Title">Optional academic or honorific title (e.g. <c>Prof.</c>).</param>
/// <param name="Salutation">Optional salutation (e.g. <c>Mr</c>, <c>Mrs</c>).</param>
/// <param name="FirstnameSuffix">Given name or contact suffix.</param>
/// <param name="AddressLine">Street address line.</param>
/// <param name="Postcode">Postal code.</param>
/// <param name="City">City name.</param>
/// <param name="CountryCode">ISO country code.</param>
/// <param name="MainContactId">Identifier of the main Bexio contact this address belongs to.</param>
/// <param name="ContactAddressId">Identifier of the Bexio contact address record this entry was copied from.</param>
public sealed record BillAddress(
    [property: JsonPropertyName("lastname_company")] string LastnameCompany,
    [property: JsonPropertyName("type")] BillAddressType Type,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("salutation")] string? Salutation = null,
    [property: JsonPropertyName("firstname_suffix")] string? FirstnameSuffix = null,
    [property: JsonPropertyName("address_line")] string? AddressLine = null,
    [property: JsonPropertyName("postcode")] string? Postcode = null,
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("country_code")] string? CountryCode = null,
    [property: JsonPropertyName("main_contact_id")] int? MainContactId = null,
    [property: JsonPropertyName("contact_address_id")] int? ContactAddressId = null
);
