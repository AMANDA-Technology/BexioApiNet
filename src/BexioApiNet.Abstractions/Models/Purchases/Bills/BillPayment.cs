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
/// Payment block attached to a bill. Same shape on create/update payloads and
/// in responses.
/// <see href="https://docs.bexio.com/#tag/Bills">Bills</see>
/// </summary>
/// <param name="Type">Payment type (<c>IBAN</c>, <c>MANUAL</c>, or <c>QR</c>).</param>
/// <param name="ExecutionDate">Execution date of the payment.</param>
/// <param name="Amount">Payment amount. Max 17 digits, 2 decimals.</param>
/// <param name="SalaryPayment">Whether this is a salary payment.</param>
/// <param name="BankAccountId">Bexio bank account identifier used for the payment.</param>
/// <param name="Fee">Fee allocation type.</param>
/// <param name="ExchangeRate">Exchange rate applied. Max 5 digits, 10 decimals.</param>
/// <param name="AccountNo">Deprecated account number.</param>
/// <param name="Iban">Receiver IBAN.</param>
/// <param name="Name">Receiver name.</param>
/// <param name="Address">Receiver full address line.</param>
/// <param name="Street">Receiver street.</param>
/// <param name="HouseNo">Receiver house number.</param>
/// <param name="Postcode">Receiver postal code.</param>
/// <param name="City">Receiver city.</param>
/// <param name="CountryCode">Receiver ISO country code.</param>
/// <param name="Message">Remittance message sent with the payment.</param>
/// <param name="BookingText">Internal booking text.</param>
/// <param name="ReferenceNo">QR reference number.</param>
/// <param name="Note">Free-form note attached to the payment.</param>
public sealed record BillPayment(
    [property: JsonPropertyName("type")] BillPaymentType Type,
    [property: JsonPropertyName("execution_date")] DateOnly ExecutionDate,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("salary_payment")] bool SalaryPayment,
    [property: JsonPropertyName("bank_account_id")] int? BankAccountId = null,
    [property: JsonPropertyName("fee")] BillPaymentFeeType? Fee = null,
    [property: JsonPropertyName("exchange_rate")] decimal? ExchangeRate = null,
    [property: JsonPropertyName("account_no")] string? AccountNo = null,
    [property: JsonPropertyName("iban")] string? Iban = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("address")] string? Address = null,
    [property: JsonPropertyName("street")] string? Street = null,
    [property: JsonPropertyName("house_no")] string? HouseNo = null,
    [property: JsonPropertyName("postcode")] string? Postcode = null,
    [property: JsonPropertyName("city")] string? City = null,
    [property: JsonPropertyName("country_code")] string? CountryCode = null,
    [property: JsonPropertyName("message")] string? Message = null,
    [property: JsonPropertyName("booking_text")] string? BookingText = null,
    [property: JsonPropertyName("reference_no")] string? ReferenceNo = null,
    [property: JsonPropertyName("note")] string? Note = null
);
