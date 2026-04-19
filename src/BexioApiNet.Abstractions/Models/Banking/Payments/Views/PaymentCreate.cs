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

using BexioApiNet.Abstractions.Models.Banking.Payments.Enums;

namespace BexioApiNet.Abstractions.Models.Banking.Payments.Views;

/// <summary>
/// Create view for a banking payment.
/// <see href="https://docs.bexio.com/#tag/Payments/operation/NewCreatePayment"/>
/// </summary>
/// <param name="Type">Type of the payment (iban or qr).</param>
/// <param name="AccountId">UUID of the Bexio sender bank account to debit.</param>
/// <param name="Recipient">Recipient (beneficiary) of the payment.</param>
/// <param name="Amount">Amount to send in the chosen currency (must be &gt; 0).</param>
/// <param name="Currency">ISO 4217 currency code (three uppercase letters).</param>
/// <param name="ExecutionDate">Execution date (should be at least the next working day).</param>
/// <param name="IsSalary">Whether this is a salary payment.</param>
/// <param name="Allowance">Fee allowance mode. Defaults to <see cref="PaymentAllowance.fee_split"/> server-side when omitted.</param>
/// <param name="QrReferenceNr">QR reference number or creditor reference number (for QR invoice payments).</param>
/// <param name="AdditionalInformation">Additional information on the payment slip.</param>
/// <param name="PurchaseReference">Reference to a purchase bill and its bill payment entry.</param>
/// <param name="IsEditingRestricted">If true, editing is restricted to the API client that created the payment.</param>
/// <param name="Message">Multi-line description of the payment.</param>
public sealed record PaymentCreate(
    [property: JsonPropertyName("type")] PaymentType Type,
    [property: JsonPropertyName("account_id")] Guid AccountId,
    [property: JsonPropertyName("recipient")] PaymentRecipient Recipient,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("execution_date")] DateOnly ExecutionDate,
    [property: JsonPropertyName("is_salary")] bool IsSalary,
    [property: JsonPropertyName("allowance")] PaymentAllowance? Allowance = null,
    [property: JsonPropertyName("qr_reference_nr")] string? QrReferenceNr = null,
    [property: JsonPropertyName("additional_information")] string? AdditionalInformation = null,
    [property: JsonPropertyName("purchase_reference")] PaymentPurchaseReference? PurchaseReference = null,
    [property: JsonPropertyName("is_editing_restricted")] bool? IsEditingRestricted = null,
    [property: JsonPropertyName("message")] string? Message = null
);
