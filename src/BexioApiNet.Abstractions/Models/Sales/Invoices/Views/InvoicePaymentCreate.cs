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

namespace BexioApiNet.Abstractions.Models.Sales.Invoices.Views;

/// <summary>
/// Create view for an invoice payment — body of <c>POST /2.0/kb_invoice/{invoice_id}/payment</c>.
/// Read-only fields (id, title, kb_invoice_id, kb_credit_voucher_id, kb_bill_id,
/// kb_credit_voucher_text, is_client_account_redemption, is_cash_discount) are intentionally
/// omitted.
/// <see href="https://docs.bexio.com/#tag/Invoice-Payments/operation/v2CreateInvoicePayment"/>
/// </summary>
/// <param name="Value">Payment amount as a formatted decimal string (required).</param>
/// <param name="Date">Payment value date in Bexio's <c>yyyy-MM-dd</c> format.</param>
/// <param name="BankAccountId">Optional reference to a bank account object.</param>
/// <param name="PaymentServiceId">External payment service used: <c>1</c> PayPal, <c>2</c> Stripe, <c>3</c> SIX Payments.</param>
public sealed record InvoicePaymentCreate(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("date")] DateOnly? Date = null,
    [property: JsonPropertyName("bank_account_id")] int? BankAccountId = null,
    [property: JsonPropertyName("payment_service_id")] int? PaymentServiceId = null
);
