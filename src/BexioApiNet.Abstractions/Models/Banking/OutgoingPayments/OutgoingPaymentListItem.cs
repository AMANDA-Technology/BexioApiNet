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
/// Condensed outgoing payment entry returned by the list endpoint
/// <c>GET /4.0/purchase/outgoing-payments</c>. See <see cref="OutgoingPayment"/> for the full model.
/// <see href="https://docs.bexio.com/#tag/Outgoing-Payment/operation/ApiOutgoingPaymentList_GET">List Outgoing Payments</see>
/// </summary>
/// <param name="Id">Unique outgoing payment identifier.</param>
/// <param name="BillId">Identifier of the bill the payment belongs to.</param>
/// <param name="PaymentType">Payment type.</param>
/// <param name="Status">Payment status.</param>
/// <param name="ExecutionDate">Execution date of the payment.</param>
/// <param name="Amount">Payment amount.</param>
/// <param name="SenderBankAccountId">Bexio bank account identifier of the sender.</param>
/// <param name="ReceiverAccountNo">Deprecated — receiver account number.</param>
/// <param name="ReceiverIban">IBAN of the receiver.</param>
/// <param name="BankingPaymentId">Reference to the banking payment order (for <c>IBAN</c>/<c>QR</c>).</param>
/// <param name="TransactionId">Reconciled transaction id when the payment is <c>RECONCILED</c>.</param>
public sealed record OutgoingPaymentListItem(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("bill_id")] Guid BillId,
    [property: JsonPropertyName("payment_type")] OutgoingPaymentType PaymentType,
    [property: JsonPropertyName("status")] OutgoingPaymentStatus Status,
    [property: JsonPropertyName("execution_date")] DateOnly ExecutionDate,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("sender_bank_account_id")] int? SenderBankAccountId,
    [property: JsonPropertyName("receiver_account_no")] string? ReceiverAccountNo,
    [property: JsonPropertyName("receiver_iban")] string? ReceiverIban,
    [property: JsonPropertyName("banking_payment_id")] Guid? BankingPaymentId,
    [property: JsonPropertyName("transaction_id")] Guid? TransactionId
);
