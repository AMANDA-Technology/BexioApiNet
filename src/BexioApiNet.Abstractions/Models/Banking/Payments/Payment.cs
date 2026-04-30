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

namespace BexioApiNet.Abstractions.Models.Banking.Payments;

/// <summary>
/// Payment as returned by the Bexio banking payments endpoints.
/// <see href="https://docs.bexio.com/#tag/Payments/operation/NewFetchAllPayments"/>
/// </summary>
/// <param name="Id">Numeric identifier of the payment.</param>
/// <param name="Uuid">Unique identifier (UUID) of the payment.</param>
/// <param name="Sender">Sender account used for the payment.</param>
/// <param name="Recipient">Recipient (beneficiary) of the payment.</param>
/// <param name="Amount">Amount to send in the chosen currency.</param>
/// <param name="Currency">ISO 4217 currency code (three uppercase letters).</param>
/// <param name="ExecutionDate">Execution date of the payment (when it should be carried out by the bank).</param>
/// <param name="Allowance">Fee allowance mode.</param>
/// <param name="IsSalary">Whether this payment is a salary payment.</param>
/// <param name="InstructionId">Optional instruction identifier assigned by the bank.</param>
/// <param name="PurchaseReference">Reference to a purchase bill and its bill payment entry, if any.</param>
/// <param name="DocumentNo">Document number of the linked purchase bill, or empty when there is no linked bill.</param>
/// <param name="QrReferenceNumber">QR IBAN or SCOR reference number (required for QR invoice payments).</param>
/// <param name="AdditionalInformation">Additional information for QR invoice payments.</param>
/// <param name="Status">Lifecycle status of the payment.</param>
/// <param name="Type">Type of the payment (iban or qr).</param>
/// <param name="DueDate">Due date for the purchase payment.</param>
/// <param name="CreatedAt">Timestamp when the payment was created (ISO 8601 with offset).</param>
/// <param name="IsEditingRestricted">If true, editing is restricted to the API client that created the payment.</param>
public sealed record Payment(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("uuid")] string? Uuid,
    [property: JsonPropertyName("sender")] PaymentSender? Sender,
    [property: JsonPropertyName("recipient")] PaymentRecipient? Recipient,
    [property: JsonPropertyName("amount")] decimal? Amount,
    [property: JsonPropertyName("currency")] string? Currency,
    [property: JsonPropertyName("execution_date")] DateOnly? ExecutionDate,
    [property: JsonPropertyName("allowance")] PaymentAllowance? Allowance,
    [property: JsonPropertyName("is_salary")] bool? IsSalary,
    [property: JsonPropertyName("instruction_id")] string? InstructionId,
    [property: JsonPropertyName("purchase_reference")] PaymentPurchaseReference? PurchaseReference,
    [property: JsonPropertyName("document_no")] string? DocumentNo,
    [property: JsonPropertyName("qr_reference_number")] string? QrReferenceNumber,
    [property: JsonPropertyName("additional_information")] string? AdditionalInformation,
    [property: JsonPropertyName("status")] PaymentStatus? Status,
    [property: JsonPropertyName("type")] PaymentType? Type,
    [property: JsonPropertyName("due_date")] DateOnly? DueDate,
    [property: JsonPropertyName("created_at")] DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("is_editing_restricted")] bool? IsEditingRestricted
);
