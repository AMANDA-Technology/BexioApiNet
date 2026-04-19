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

namespace BexioApiNet.Abstractions.Models.Contacts.Contacts;

/// <summary>
/// Contact as returned by the Bexio contacts endpoint. Covers both the plain <c>Contact</c>
/// schema and the <c>ContactWithDetails</c> variant (which adds <c>profile_image</c>).
/// <see href="https://docs.bexio.com/#tag/Contacts/operation/v2ListContacts"/>
/// </summary>
/// <param name="Id">Unique contact identifier (read-only).</param>
/// <param name="Nr">Optional customer number (string of digits). Bexio assigns one automatically when <see langword="null"/>.</param>
/// <param name="ContactTypeId"><c>1</c> for companies, <c>2</c> for persons.</param>
/// <param name="Name1">Company name if <see cref="ContactTypeId"/> is <c>1</c>, otherwise the person's last name.</param>
/// <param name="Name2">Company addition if <see cref="ContactTypeId"/> is <c>1</c>, otherwise the person's first name.</param>
/// <param name="SalutationId">Reference to a salutation object.</param>
/// <param name="SalutationForm">Salutation form id.</param>
/// <param name="TitleId">Reference to a title object (read-only — use <c>titel_id</c> for writes).</param>
/// <param name="Birthday">Birthday of the contact.</param>
/// <param name="Address">Composed address string returned by Bexio (read-only).</param>
/// <param name="StreetName">Street part of the postal address.</param>
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
/// <param name="IsLead">Legacy lead flag (deprecated, read-only).</param>
/// <param name="ContactGroupIds">Comma-separated references to one or more contact group objects.</param>
/// <param name="ContactBranchIds">Comma-separated references to one or more contact sector objects.</param>
/// <param name="UserId">Reference to the user that owns the contact.</param>
/// <param name="OwnerId">Reference to the owner of the contact.</param>
/// <param name="UpdatedAt">Timestamp of the last update in Bexio's <c>yyyy-MM-dd HH:mm:ss</c> format (read-only).</param>
/// <param name="ProfileImage">Base64-encoded profile image. Only populated on the <c>ContactWithDetails</c> responses (show / create / update).</param>
public sealed record Contact(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("nr")] string? Nr,
    [property: JsonPropertyName("contact_type_id")] int ContactTypeId,
    [property: JsonPropertyName("name_1")] string Name1,
    [property: JsonPropertyName("name_2")] string? Name2,
    [property: JsonPropertyName("salutation_id")] int? SalutationId,
    [property: JsonPropertyName("salutation_form")] int? SalutationForm,
    [property: JsonPropertyName("title_id")] int? TitleId,
    [property: JsonPropertyName("birthday")] DateOnly? Birthday,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("street_name")] string? StreetName,
    [property: JsonPropertyName("house_number")] string? HouseNumber,
    [property: JsonPropertyName("address_addition")] string? AddressAddition,
    [property: JsonPropertyName("postcode")] string? Postcode,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("country_id")] int? CountryId,
    [property: JsonPropertyName("mail")] string? Mail,
    [property: JsonPropertyName("mail_second")] string? MailSecond,
    [property: JsonPropertyName("phone_fixed")] string? PhoneFixed,
    [property: JsonPropertyName("phone_fixed_second")] string? PhoneFixedSecond,
    [property: JsonPropertyName("phone_mobile")] string? PhoneMobile,
    [property: JsonPropertyName("fax")] string? Fax,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("skype_name")] string? SkypeName,
    [property: JsonPropertyName("remarks")] string? Remarks,
    [property: JsonPropertyName("language_id")] int? LanguageId,
    [property: JsonPropertyName("is_lead")] bool? IsLead,
    [property: JsonPropertyName("contact_group_ids")] string? ContactGroupIds,
    [property: JsonPropertyName("contact_branch_ids")] string? ContactBranchIds,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("owner_id")] int OwnerId,
    [property: JsonPropertyName("updated_at")] string? UpdatedAt,
    [property: JsonPropertyName("profile_image")] string? ProfileImage
);
