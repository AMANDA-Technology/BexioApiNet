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

using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Enums;

namespace BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Views;

/// <summary>
/// Create view for <c>POST /4.0/purchase/outgoing-payments</c>.
/// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPayment_POST">Create Outgoing Payment</see>
/// </summary>
/// <param name="BillId">Identifier of the bill. Payment can only be created for bills that are not in <c>DRAFT</c> status.</param>
/// <param name="PaymentType">Payment type. Bill amounts cannot be covered by <c>CASH_DISCOUNT</c> payments only.</param>
/// <param name="ExecutionDate">Execution date. Must be within an existing (non-closed, non-locked) business year; for <c>IBAN</c>/<c>QR</c> it must be in the present or future and not on a weekend.</param>
/// <param name="Amount">Payment amount. Must be less or equal to the bill's pending amount; maximum 17 digits and 2 decimals.</param>
/// <param name="CurrencyCode">Currency code. Must equal the bill's currency. Only <c>CHF</c> and <c>EUR</c> are allowed for <c>QR</c>.</param>
/// <param name="ExchangeRate">Exchange rate. Maximum 5 digits and 10 decimals.</param>
/// <param name="SenderBankAccountId">Bexio bank account identifier of the sender. Required for <c>IBAN</c>/<c>MANUAL</c>/<c>QR</c>; not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="IsSalaryPayment">Whether this is a salary payment. Only allowed to be <c>true</c> for <c>IBAN</c>.</param>
/// <param name="Note">Optional note. Not allowed for <c>IBAN</c>/<c>QR</c>.</param>
/// <param name="SenderIban">Sender IBAN. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderName">Sender name. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderStreet">Sender street. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderHouseNo">Sender house number. Not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderCity">Sender city. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderPostcode">Sender postcode. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderCountryCode">Sender country code. Not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderBcNo">Sender bank clearing number. Not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderBankNo">Sender bank number. Not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="SenderBankName">Sender bank name. Not allowed for <c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverAccountNo">Deprecated — receiver account number.</param>
/// <param name="ReceiverIban">Receiver IBAN. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>. For <c>QR</c>, must be a valid QR IBAN.</param>
/// <param name="ReceiverName">Receiver name. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverStreet">Receiver street. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverHouseNo">Receiver house number. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverCity">Receiver city. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverPostcode">Receiver postcode. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverCountryCode">Receiver country code. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverBcNo">Receiver bank clearing number. Not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverBankNo">Receiver bank number. Not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverBankName">Receiver bank name. Not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="FeeType">Fee allocation. Required for <c>IBAN</c>; not allowed for <c>QR</c>/<c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReferenceNo">Reference number (for <c>QR</c>).</param>
/// <param name="Message">Remittance message. Not allowed for <c>QR</c>/<c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="BookingText">Internal booking text. Not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
public sealed record OutgoingPaymentCreate(
    [property: JsonPropertyName("bill_id")] Guid BillId,
    [property: JsonPropertyName("payment_type")] OutgoingPaymentType PaymentType,
    [property: JsonPropertyName("execution_date")] DateOnly ExecutionDate,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("exchange_rate")] decimal ExchangeRate,
    [property: JsonPropertyName("sender_bank_account_id")] int SenderBankAccountId,
    [property: JsonPropertyName("is_salary_payment")] bool IsSalaryPayment,
    [property: JsonPropertyName("note")] string? Note = null,
    [property: JsonPropertyName("sender_iban")] string? SenderIban = null,
    [property: JsonPropertyName("sender_name")] string? SenderName = null,
    [property: JsonPropertyName("sender_street")] string? SenderStreet = null,
    [property: JsonPropertyName("sender_house_no")] string? SenderHouseNo = null,
    [property: JsonPropertyName("sender_city")] string? SenderCity = null,
    [property: JsonPropertyName("sender_postcode")] string? SenderPostcode = null,
    [property: JsonPropertyName("sender_country_code")] string? SenderCountryCode = null,
    [property: JsonPropertyName("sender_bc_no")] string? SenderBcNo = null,
    [property: JsonPropertyName("sender_bank_no")] string? SenderBankNo = null,
    [property: JsonPropertyName("sender_bank_name")] string? SenderBankName = null,
    [property: JsonPropertyName("receiver_account_no")] string? ReceiverAccountNo = null,
    [property: JsonPropertyName("receiver_iban")] string? ReceiverIban = null,
    [property: JsonPropertyName("receiver_name")] string? ReceiverName = null,
    [property: JsonPropertyName("receiver_street")] string? ReceiverStreet = null,
    [property: JsonPropertyName("receiver_house_no")] string? ReceiverHouseNo = null,
    [property: JsonPropertyName("receiver_city")] string? ReceiverCity = null,
    [property: JsonPropertyName("receiver_postcode")] string? ReceiverPostcode = null,
    [property: JsonPropertyName("receiver_country_code")] string? ReceiverCountryCode = null,
    [property: JsonPropertyName("receiver_bc_no")] string? ReceiverBcNo = null,
    [property: JsonPropertyName("receiver_bank_no")] string? ReceiverBankNo = null,
    [property: JsonPropertyName("receiver_bank_name")] string? ReceiverBankName = null,
    [property: JsonPropertyName("fee_type")] OutgoingPaymentFeeType? FeeType = null,
    [property: JsonPropertyName("reference_no")] string? ReferenceNo = null,
    [property: JsonPropertyName("message")] string? Message = null,
    [property: JsonPropertyName("booking_text")] string? BookingText = null
);
