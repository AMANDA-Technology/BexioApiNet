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

namespace BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;

/// <summary>
/// Update view for a contact — body of <c>POST /2.0/contact/{id}</c>. The Bexio API uses
/// <c>POST</c> (not <c>PUT</c>) for full-replacement edits on this resource and requires
/// the same fields as <see cref="ContactCreate"/>.
/// <see href="https://docs.bexio.com/#tag/Contacts/operation/v2EditContact"/>
/// </summary>
/// <param name="ContactTypeId"><c>1</c> for companies, <c>2</c> for persons.</param>
/// <param name="Name1">Company name if <see cref="ContactTypeId"/> is <c>1</c>, otherwise the person's last name.</param>
/// <param name="UserId">Reference to the user that owns the contact.</param>
/// <param name="OwnerId">Reference to the owner of the contact.</param>
/// <param name="Nr">Optional customer number. Bexio assigns one automatically when <see langword="null"/>.</param>
/// <param name="Name2">Company addition if <see cref="ContactTypeId"/> is <c>1</c>, otherwise the person's first name.</param>
/// <param name="SalutationId">Reference to a salutation object.</param>
/// <param name="SalutationForm">Salutation form id.</param>
/// <param name="TitelId">Reference to a title object (write-only; Bexio uses the spelling <c>titel_id</c>).</param>
/// <param name="Birthday">Birthday of the contact.</param>
/// <param name="StreetName">Street part of the postal address. Required when <see cref="HouseNumber"/> or <see cref="AddressAddition"/> is provided.</param>
/// <param name="HouseNumber">House number part of the postal address.</param>
/// <param name="AddressAddition">Additional address line (e.g. building, c/o).</param>
/// <param name="Postcode">Postal code.</param>
/// <param name="City">City or locality.</param>
/// <param name="CountryId">Reference to a country object.</param>
/// <param name="Mail">Primary email address.</param>
/// <param name="MailSecond">Secondary email address.</param>
/// <param name="PhoneFixed">Primary landline phone number.</param>
/// <param name="PhoneFixedSecond">Secondary landline phone number.</param>
/// <param name="PhoneMobile">Mobile phone number.</param>
/// <param name="Fax">Fax number.</param>
/// <param name="Url">Website URL.</param>
/// <param name="SkypeName">Skype name.</param>
/// <param name="Remarks">Free-text remarks.</param>
/// <param name="LanguageId">Reference to a language object.</param>
/// <param name="ContactGroupIds">Comma-separated references to one or more contact group objects.</param>
/// <param name="ContactBranchIds">Comma-separated references to one or more contact sector objects.</param>
public sealed record ContactUpdate(
    [property: JsonPropertyName("contact_type_id")] int ContactTypeId,
    [property: JsonPropertyName("name_1")] string Name1,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("owner_id")] int OwnerId,
    [property: JsonPropertyName("nr")] string? Nr = null,
    [property: JsonPropertyName("name_2")] string? Name2 = null,
    [property: JsonPropertyName("salutation_id")] int? SalutationId = null,
    [property: JsonPropertyName("salutation_form")] int? SalutationForm = null,
    [property: JsonPropertyName("titel_id")] int? TitelId = null,
    [property: JsonPropertyName("birthday")] DateOnly? Birthday = null,
    [property: JsonPropertyName("street_name")] string? StreetName = null,
    [property: JsonPropertyName("house_number")] string? HouseNumber = null,
    [property: JsonPropertyName("address_addition")] string? AddressAddition = null,
    [property: JsonPropertyName("postcode")] string? Postcode = null,
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("country_id")] int? CountryId = null,
    [property: JsonPropertyName("mail")] string? Mail = null,
    [property: JsonPropertyName("mail_second")] string? MailSecond = null,
    [property: JsonPropertyName("phone_fixed")] string? PhoneFixed = null,
    [property: JsonPropertyName("phone_fixed_second")] string? PhoneFixedSecond = null,
    [property: JsonPropertyName("phone_mobile")] string? PhoneMobile = null,
    [property: JsonPropertyName("fax")] string? Fax = null,
    [property: JsonPropertyName("url")] string? Url = null,
    [property: JsonPropertyName("skype_name")] string? SkypeName = null,
    [property: JsonPropertyName("remarks")] string? Remarks = null,
    [property: JsonPropertyName("language_id")] int? LanguageId = null,
    [property: JsonPropertyName("contact_group_ids")] string? ContactGroupIds = null,
    [property: JsonPropertyName("contact_branch_ids")] string? ContactBranchIds = null
);
