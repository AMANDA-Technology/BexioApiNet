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

namespace BexioApiNet.Abstractions.Models.Banking.OutgoingPayments;

/// <summary>
/// Outgoing payment returned by the Bexio <c>/4.0/purchase/outgoing-payments</c> endpoints
/// (GET by id, POST create, PUT update). <see href="https://docs.bexio.com/#tag/Outgoing-Payment">Outgoing Payment</see>
/// </summary>
/// <param name="Id">Unique outgoing payment identifier.</param>
/// <param name="Status">Payment status (<c>PENDING</c>, <c>TRANSFERRED</c>, ...).</param>
/// <param name="CreatedAt">Timestamp when the payment was created.</param>
/// <param name="BillId">Identifier of the bill the payment belongs to.</param>
/// <param name="PaymentType">Payment type (<c>IBAN</c>, <c>MANUAL</c>, <c>CASH_DISCOUNT</c>, <c>RECONCILED</c>, <c>QR</c>).</param>
/// <param name="ExecutionDate">Execution date of the payment.</param>
/// <param name="Amount">Payment amount.</param>
/// <param name="CurrencyCode">ISO currency code of the payment.</param>
/// <param name="ExchangeRate">Exchange rate used for the payment.</param>
/// <param name="Note">Optional note — not allowed for <c>IBAN</c> or <c>QR</c> payments.</param>
/// <param name="SenderBankAccountId">Bexio bank account identifier of the sender.</param>
/// <param name="SenderIban">IBAN of the sender.</param>
/// <param name="SenderName">Name of the sender.</param>
/// <param name="SenderStreet">Street of the sender.</param>
/// <param name="SenderHouseNo">House number of the sender.</param>
/// <param name="SenderCity">City of the sender.</param>
/// <param name="SenderPostcode">Postcode of the sender.</param>
/// <param name="SenderCountryCode">Country code of the sender.</param>
/// <param name="SenderBcNo">Bank clearing number of the sender bank.</param>
/// <param name="SenderBankNo">Bank number of the sender bank.</param>
/// <param name="SenderBankName">Name of the sender bank.</param>
/// <param name="ReceiverAccountNo">Deprecated — receiver account number.</param>
/// <param name="ReceiverIban">IBAN of the receiver.</param>
/// <param name="ReceiverName">Name of the receiver.</param>
/// <param name="ReceiverStreet">Street of the receiver.</param>
/// <param name="ReceiverHouseNo">House number of the receiver.</param>
/// <param name="ReceiverCity">City of the receiver.</param>
/// <param name="ReceiverPostcode">Postcode of the receiver.</param>
/// <param name="ReceiverCountryCode">Country code of the receiver.</param>
/// <param name="ReceiverBcNo">Bank clearing number of the receiver bank.</param>
/// <param name="ReceiverBankNo">Bank number of the receiver bank.</param>
/// <param name="ReceiverBankName">Name of the receiver bank.</param>
/// <param name="FeeType">Fee allocation (<c>BY_SENDER</c>, <c>BY_RECEIVER</c>, <c>BREAKDOWN</c>, <c>NO_FEE</c>).</param>
/// <param name="IsSalaryPayment">True if this is a salary payment. Only allowed for <c>IBAN</c>.</param>
/// <param name="ReferenceNo">Reference number (for <c>QR</c> payments).</param>
/// <param name="Message">Remittance message.</param>
/// <param name="BookingText">Internal booking text.</param>
/// <param name="BankingPaymentId">Reference to the banking payment order (for <c>IBAN</c>/<c>QR</c>).</param>
/// <param name="BankingPaymentEntryId">Reference to the banking payment order entry.</param>
/// <param name="TransactionId">Reconciled transaction id when the payment is <c>RECONCILED</c>.</param>
public sealed record OutgoingPayment(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("status")] OutgoingPaymentStatus Status,
    [property: JsonPropertyName("created_at")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("bill_id")] Guid BillId,
    [property: JsonPropertyName("payment_type")] OutgoingPaymentType PaymentType,
    [property: JsonPropertyName("execution_date")] DateOnly ExecutionDate,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("exchange_rate")] decimal ExchangeRate,
    [property: JsonPropertyName("note")] string? Note,
    [property: JsonPropertyName("sender_bank_account_id")] int SenderBankAccountId,
    [property: JsonPropertyName("sender_iban")] string? SenderIban,
    [property: JsonPropertyName("sender_name")] string? SenderName,
    [property: JsonPropertyName("sender_street")] string? SenderStreet,
    [property: JsonPropertyName("sender_house_no")] string? SenderHouseNo,
    [property: JsonPropertyName("sender_city")] string? SenderCity,
    [property: JsonPropertyName("sender_postcode")] string? SenderPostcode,
    [property: JsonPropertyName("sender_country_code")] string? SenderCountryCode,
    [property: JsonPropertyName("sender_bc_no")] string? SenderBcNo,
    [property: JsonPropertyName("sender_bank_no")] string? SenderBankNo,
    [property: JsonPropertyName("sender_bank_name")] string? SenderBankName,
    [property: JsonPropertyName("receiver_account_no")] string? ReceiverAccountNo,
    [property: JsonPropertyName("receiver_iban")] string? ReceiverIban,
    [property: JsonPropertyName("receiver_name")] string? ReceiverName,
    [property: JsonPropertyName("receiver_street")] string? ReceiverStreet,
    [property: JsonPropertyName("receiver_house_no")] string? ReceiverHouseNo,
    [property: JsonPropertyName("receiver_city")] string? ReceiverCity,
    [property: JsonPropertyName("receiver_postcode")] string? ReceiverPostcode,
    [property: JsonPropertyName("receiver_country_code")] string? ReceiverCountryCode,
    [property: JsonPropertyName("receiver_bc_no")] string? ReceiverBcNo,
    [property: JsonPropertyName("receiver_bank_no")] string? ReceiverBankNo,
    [property: JsonPropertyName("receiver_bank_name")] string? ReceiverBankName,
    [property: JsonPropertyName("fee_type")] OutgoingPaymentFeeType? FeeType,
    [property: JsonPropertyName("is_salary_payment")] bool IsSalaryPayment,
    [property: JsonPropertyName("reference_no")] string? ReferenceNo,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("booking_text")] string? BookingText,
    [property: JsonPropertyName("banking_payment_id")] Guid? BankingPaymentId,
    [property: JsonPropertyName("banking_payment_entry_id")] Guid? BankingPaymentEntryId,
    [property: JsonPropertyName("transaction_id")] Guid? TransactionId
);
