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
/// Update view for <c>PUT /4.0/purchase/outgoing-payments</c>. The target payment is identified by
/// <see cref="PaymentId"/> in the body — the Bexio spec does not use <c>{id}</c> in the request path for this endpoint.
/// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPayment_PUT">Edit Outgoing Payment</see>
/// </summary>
/// <param name="PaymentId">Identifier of the outgoing payment to update.</param>
/// <param name="ExecutionDate">Execution date. Must be within an existing business year; for <c>IBAN</c>/<c>QR</c> it must be in the present or future and not on a weekend.</param>
/// <param name="Amount">Payment amount. Must be less or equal to the bill's pending amount; maximum 17 digits and 2 decimals.</param>
/// <param name="IsSalaryPayment">Whether this is a salary payment. Only allowed to be <c>true</c> for <c>IBAN</c>.</param>
/// <param name="FeeType">Fee allocation. Required for <c>IBAN</c>; not allowed for <c>QR</c>/<c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReferenceNo">Reference number. For <c>QR</c> payments, must be a valid ISR account or creditor reference.</param>
/// <param name="Message">Remittance message. Not allowed for <c>QR</c>/<c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverIban">Receiver IBAN. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverName">Receiver name. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverStreet">Receiver street. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverHouseNo">Receiver house number. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverCity">Receiver city. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverPostcode">Receiver postcode. Required for <c>IBAN</c>/<c>QR</c>; not allowed for <c>MANUAL</c>/<c>CASH_DISCOUNT</c>.</param>
/// <param name="ReceiverCountryCode">Receiver country code.</param>
public sealed record OutgoingPaymentUpdate(
    [property: JsonPropertyName("payment_id")] Guid PaymentId,
    [property: JsonPropertyName("execution_date")] DateOnly ExecutionDate,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("is_salary_payment")] bool IsSalaryPayment,
    [property: JsonPropertyName("fee_type")] OutgoingPaymentFeeType? FeeType = null,
    [property: JsonPropertyName("reference_no")] string? ReferenceNo = null,
    [property: JsonPropertyName("message")] string? Message = null,
    [property: JsonPropertyName("receiver_iban")] string? ReceiverIban = null,
    [property: JsonPropertyName("receiver_name")] string? ReceiverName = null,
    [property: JsonPropertyName("receiver_street")] string? ReceiverStreet = null,
    [property: JsonPropertyName("receiver_house_no")] string? ReceiverHouseNo = null,
    [property: JsonPropertyName("receiver_city")] string? ReceiverCity = null,
    [property: JsonPropertyName("receiver_postcode")] string? ReceiverPostcode = null,
    [property: JsonPropertyName("receiver_country_code")] string? ReceiverCountryCode = null
);
