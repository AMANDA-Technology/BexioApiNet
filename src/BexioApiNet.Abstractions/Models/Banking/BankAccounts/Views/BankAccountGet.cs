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

using BexioApiNet.Abstractions.Models.Banking.BankAccounts.Enums;

namespace BexioApiNet.Abstractions.Models.Banking.BankAccounts.Views;

/// <summary>
/// Bexio bank account get view returned by <c>GET /3.0/banking/accounts</c> and
/// <c>GET /3.0/banking/accounts/{bank_account_id}</c>.
/// <see href="https://docs.bexio.com/#tag/Bank-Accounts/operation/ShowBankAccount">Show Bank Account</see>
/// </summary>
/// <param name="Id">Identifier of the bank account.</param>
/// <param name="Name">Name of the bank account.</param>
/// <param name="Owner">Account holder name.</param>
/// <param name="OwnerAddress">Account holder street name.</param>
/// <param name="OwnerHouseNumber">Account holder house number.</param>
/// <param name="OwnerZip">Account holder postal code.</param>
/// <param name="OwnerCity">Account holder city.</param>
/// <param name="OwnerCountryCode">ISO 3166-1 alpha-2 country code of the account holder.</param>
/// <param name="BcNr">Bank clearing number (Swiss BC number).</param>
/// <param name="BankName">Name of the bank.</param>
/// <param name="BankNr">Bank number.</param>
/// <param name="BankAccountNr">Domestic bank account number.</param>
/// <param name="IbanNr">IBAN of the bank account.</param>
/// <param name="CurrencyId">Identifier of the currency the account is held in.</param>
/// <param name="AccountId">Identifier of the linked accounting account.</param>
/// <param name="Remarks">Free-form remarks for the bank account.</param>
/// <param name="QrInvoiceIban">Optional QR-IBAN used for QR-bills.</param>
/// <param name="InvoiceMode">QR invoice mode of the bank account (e.g. <c>qr_iban</c>).</param>
/// <param name="IsEsr">Whether the orange inpayment slip (ISR/ESR) is activated. Deprecated by Bexio.</param>
/// <param name="EsrBesrId">ISR/ESR participant number (BESR ID).</param>
/// <param name="EsrPostAccountNr">PostFinance account number used for ISR/ESR slips.</param>
/// <param name="EsrPaymentForText">Text shown on the "payment for" line of ISR/ESR slips.</param>
/// <param name="EsrInFavourOfText">Text shown on the "in favour of" line of ISR/ESR slips.</param>
/// <param name="Type">Type of the bank account. Always <c>"bank"</c> for this endpoint.</param>
public sealed record BankAccountGet(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("owner")] string? Owner,
    [property: JsonPropertyName("owner_address")] string? OwnerAddress,
    [property: JsonPropertyName("owner_house_number")] string? OwnerHouseNumber,
    [property: JsonPropertyName("owner_zip")] string? OwnerZip,
    [property: JsonPropertyName("owner_city")] string? OwnerCity,
    [property: JsonPropertyName("owner_country_code")] string? OwnerCountryCode,
    [property: JsonPropertyName("bc_nr")] string? BcNr,
    [property: JsonPropertyName("bank_name")] string? BankName,
    [property: JsonPropertyName("bank_nr")] string? BankNr,
    [property: JsonPropertyName("bank_account_nr")] string? BankAccountNr,
    [property: JsonPropertyName("iban_nr")] string? IbanNr,
    [property: JsonPropertyName("currency_id")] int? CurrencyId,
    [property: JsonPropertyName("account_id")] int? AccountId,
    [property: JsonPropertyName("remarks")] string? Remarks,
    [property: JsonPropertyName("qr_invoice_iban")] string? QrInvoiceIban,
    [property: JsonPropertyName("invoice_mode")] BankAccountInvoiceMode? InvoiceMode,
    [property: JsonPropertyName("is_esr")] bool? IsEsr,
    [property: JsonPropertyName("esr_besr_id")] string? EsrBesrId,
    [property: JsonPropertyName("esr_post_account_nr")] string? EsrPostAccountNr,
    [property: JsonPropertyName("esr_payment_for_text")] string? EsrPaymentForText,
    [property: JsonPropertyName("esr_in_favour_of_text")] string? EsrInFavourOfText,
    [property: JsonPropertyName("type")] string? Type
);
