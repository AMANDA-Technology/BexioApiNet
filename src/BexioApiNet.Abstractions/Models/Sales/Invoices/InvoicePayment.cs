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

namespace BexioApiNet.Abstractions.Models.Sales.Invoices;

/// <summary>
/// Payment applied to an <see cref="Invoice"/> via <c>/2.0/kb_invoice/{invoice_id}/payment</c>.
/// <see href="https://docs.bexio.com/#tag/Invoice-Payments/operation/v2ListInvoicePayments"/>
/// </summary>
/// <param name="Id">Unique payment identifier (read-only).</param>
/// <param name="Date">Payment value date in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="Value">Payment amount as a formatted decimal string (required).</param>
/// <param name="BankAccountId">Optional reference to a bank account object.</param>
/// <param name="Title">Read-only payment title (e.g. <c>Received Payment</c>).</param>
/// <param name="PaymentServiceId">External payment service used: <c>1</c> PayPal, <c>2</c> Stripe, <c>3</c> SIX Payments.</param>
/// <param name="IsClientAccountRedemption">Read-only flag — <see langword="true"/> when the payment redeems a client account balance.</param>
/// <param name="IsCashDiscount">Read-only flag — <see langword="true"/> when the payment represents a cash discount.</param>
/// <param name="KbInvoiceId">Read-only reference to the invoice the payment belongs to.</param>
/// <param name="KbCreditVoucherId">Read-only reference to a credit voucher if the payment originates from one.</param>
/// <param name="KbBillId">Read-only reference to a bill if the payment originates from one.</param>
/// <param name="KbCreditVoucherText">Read-only credit voucher description text.</param>
public sealed record InvoicePayment(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("date")] DateOnly? Date,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("bank_account_id")] int? BankAccountId,
    [property: JsonPropertyName("title")] string? Title,
    [property: JsonPropertyName("payment_service_id")] int? PaymentServiceId,
    [property: JsonPropertyName("is_client_account_redemption")] bool? IsClientAccountRedemption,
    [property: JsonPropertyName("is_cash_discount")] bool? IsCashDiscount,
    [property: JsonPropertyName("kb_invoice_id")] int? KbInvoiceId,
    [property: JsonPropertyName("kb_credit_voucher_id")] int? KbCreditVoucherId,
    [property: JsonPropertyName("kb_bill_id")] int? KbBillId,
    [property: JsonPropertyName("kb_credit_voucher_text")] string? KbCreditVoucherText
);
