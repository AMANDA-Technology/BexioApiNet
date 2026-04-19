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

namespace BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses.Views;

/// <summary>
/// Create view for an additional address. <see href="https://docs.bexio.com/#tag/Additional-Addresses/operation/v2CreateAdditionalAddress"/>
/// </summary>
/// <param name="Name">Display name of the additional address.</param>
/// <param name="NameAddition">Optional name addition / suffix.</param>
/// <param name="StreetName">Street name. Is required if <c>house_number</c> or <c>address_addition</c> are not <c>null</c>.</param>
/// <param name="HouseNumber">House number. Requires <c>street_name</c> when not <c>null</c>.</param>
/// <param name="AddressAddition">Optional address addition. Requires <c>street_name</c> when not <c>null</c>.</param>
/// <param name="Postcode">Postal code of the additional address.</param>
/// <param name="City">City of the additional address.</param>
/// <param name="CountryId">Country identifier. References a country object.</param>
/// <param name="Subject">Subject line describing the additional address.</param>
/// <param name="Description">Free-form internal description of the additional address.</param>
public sealed record AdditionalAddressCreate(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("name_addition")] string? NameAddition,
    [property: JsonPropertyName("street_name")] string? StreetName,
    [property: JsonPropertyName("house_number")] string? HouseNumber,
    [property: JsonPropertyName("address_addition")] string? AddressAddition,
    [property: JsonPropertyName("postcode")] string? Postcode,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("country_id")] int? CountryId,
    [property: JsonPropertyName("subject")] string? Subject,
    [property: JsonPropertyName("description")] string? Description
);
