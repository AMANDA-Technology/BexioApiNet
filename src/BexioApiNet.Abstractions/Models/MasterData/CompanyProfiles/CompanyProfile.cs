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

using BexioApiNet.Abstractions.Models.MasterData.CompanyProfiles.Enums;

namespace BexioApiNet.Abstractions.Models.MasterData.CompanyProfiles;

/// <summary>
/// Company profile object returned by <c>/2.0/company_profile</c>. Each Bexio account
/// currently owns a single profile but the list endpoint still returns it as an array.
/// <see href="https://docs.bexio.com/#tag/Company-Profile"/>
/// </summary>
public sealed record CompanyProfile
{
    /// <summary>
    /// Unique company profile identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    /// Display name of the company.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Street address.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }

    /// <summary>
    /// Additional address line (e.g. mail box). Despite the field name, the Bexio API states
    /// this is not the street number but an additional address line.
    /// </summary>
    [JsonPropertyName("address_nr")]
    public string? AddressNr { get; init; }

    /// <summary>
    /// Postal code.
    /// </summary>
    [JsonPropertyName("postcode")]
    public string? Postcode { get; init; }

    /// <summary>
    /// City / town name.
    /// </summary>
    [JsonPropertyName("city")]
    public string? City { get; init; }

    /// <summary>
    /// Reference to a <see href="https://docs.bexio.com/#tag/Countries/operation/v2ListCountries">country object</see> by id.
    /// </summary>
    [JsonPropertyName("country_id")]
    public int? CountryId { get; init; }

    /// <summary>
    /// Legal form of the company.
    /// </summary>
    [JsonPropertyName("legal_form")]
    public CompanyLegalForm? LegalForm { get; init; }

    /// <summary>
    /// Display name of the country (denormalised by Bexio).
    /// </summary>
    [JsonPropertyName("country_name")]
    public string? CountryName { get; init; }

    /// <summary>
    /// Contact e-mail address.
    /// </summary>
    [JsonPropertyName("mail")]
    public string? Mail { get; init; }

    /// <summary>
    /// Primary fixed-line phone number.
    /// </summary>
    [JsonPropertyName("phone_fixed")]
    public string? PhoneFixed { get; init; }

    /// <summary>
    /// Primary mobile phone number.
    /// </summary>
    [JsonPropertyName("phone_mobile")]
    public string? PhoneMobile { get; init; }

    /// <summary>
    /// Fax number.
    /// </summary>
    [JsonPropertyName("fax")]
    public string? Fax { get; init; }

    /// <summary>
    /// Public website URL.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>
    /// Skype contact name.
    /// </summary>
    [JsonPropertyName("skype_name")]
    public string? SkypeName { get; init; }

    /// <summary>
    /// Facebook contact name.
    /// </summary>
    [JsonPropertyName("facebook_name")]
    public string? FacebookName { get; init; }

    /// <summary>
    /// Twitter contact name.
    /// </summary>
    [JsonPropertyName("twitter_name")]
    public string? TwitterName { get; init; }

    /// <summary>
    /// Free-form company description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// UID / UST-ID company number.
    /// </summary>
    [JsonPropertyName("ust_id_nr")]
    public string? UstIdNr { get; init; }

    /// <summary>
    /// VAT (MWST) number.
    /// </summary>
    [JsonPropertyName("mwst_nr")]
    public string? MwstNr { get; init; }

    /// <summary>
    /// Commercial / trade register number.
    /// </summary>
    [JsonPropertyName("trade_register_nr")]
    public string? TradeRegisterNr { get; init; }

    /// <summary>
    /// <see langword="true"/> if the company uses its own logo.
    /// </summary>
    [JsonPropertyName("has_own_logo")]
    public bool? HasOwnLogo { get; init; }

    /// <summary>
    /// <see langword="true"/> if the profile is shown publicly on bexio's platform.
    /// </summary>
    [JsonPropertyName("is_public_profile")]
    public bool? IsPublicProfile { get; init; }

    /// <summary>
    /// <see langword="true"/> if the logo is public.
    /// </summary>
    [JsonPropertyName("is_logo_public")]
    public bool? IsLogoPublic { get; init; }

    /// <summary>
    /// <see langword="true"/> if the postal address is public.
    /// </summary>
    [JsonPropertyName("is_address_public")]
    public bool? IsAddressPublic { get; init; }

    /// <summary>
    /// <see langword="true"/> if the fixed-line phone is public.
    /// </summary>
    [JsonPropertyName("is_phone_public")]
    public bool? IsPhonePublic { get; init; }

    /// <summary>
    /// <see langword="true"/> if the mobile phone is public.
    /// </summary>
    [JsonPropertyName("is_mobile_public")]
    public bool? IsMobilePublic { get; init; }

    /// <summary>
    /// <see langword="true"/> if the fax number is public.
    /// </summary>
    [JsonPropertyName("is_fax_public")]
    public bool? IsFaxPublic { get; init; }

    /// <summary>
    /// <see langword="true"/> if the e-mail address is public.
    /// </summary>
    [JsonPropertyName("is_mail_public")]
    public bool? IsMailPublic { get; init; }

    /// <summary>
    /// <see langword="true"/> if the website URL is public.
    /// </summary>
    [JsonPropertyName("is_url_public")]
    public bool? IsUrlPublic { get; init; }

    /// <summary>
    /// <see langword="true"/> if the Skype name is public.
    /// </summary>
    [JsonPropertyName("is_skype_public")]
    public bool? IsSkypePublic { get; init; }

    /// <summary>
    /// Base64-encoded logo image bytes.
    /// </summary>
    [JsonPropertyName("logo_base64")]
    public string? LogoBase64 { get; init; }
}
