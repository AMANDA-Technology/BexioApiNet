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

using BexioApiNet.Abstractions.Models.Expenses.Expenses.Enums;

namespace BexioApiNet.Abstractions.Models.Expenses.Expenses;

/// <summary>
/// Condensed expense entry returned by the list endpoint
/// <c>GET /4.0/expenses</c>. See <see cref="Expense"/> for the full model returned
/// by the single-item endpoints.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="Id">Unique expense identifier.</param>
/// <param name="DocumentNo">Unique expense document number (max length 255).</param>
/// <param name="Status">Expense status (<c>DRAFT</c> or <c>DONE</c>).</param>
/// <param name="CreatedAt">Creation timestamp (date-time).</param>
/// <param name="CurrencyCode">ISO currency code (max length 20).</param>
/// <param name="PaidOn">Date the expense was paid on.</param>
/// <param name="Gross">Gross value of the expense — calculated server-side from <c>amount</c> and <c>tax_id</c>.</param>
/// <param name="Net">Net value of the expense — calculated server-side from <c>amount</c> and <c>tax_id</c>.</param>
/// <param name="AttachmentIds">Bexio file identifiers attached to the expense.</param>
/// <param name="Title">Optional free-form title (max length 80).</param>
/// <param name="FirstnameSuffix">Supplier first name or suffix (max length 80).</param>
/// <param name="LastnameCompany">Supplier family name or company name (max length 80).</param>
/// <param name="Vendor">Joined <c>firstname_suffix</c> + <c>lastname_company</c> based on supplier (contact) type.</param>
/// <param name="BookingAccountId">Booking account identifier.</param>
/// <param name="ProjectId">Linked project identifier.</param>
/// <param name="ChargeableContactId">Identifier of the chargeable contact.</param>
/// <param name="TransactionId">Reconciled transaction identifier (when this expense is reconciled).</param>
/// <param name="InvoiceId">Linked invoice identifier (when this expense is linked to an invoice).</param>
public sealed record ExpenseListItem(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("document_no")] string DocumentNo,
    [property: JsonPropertyName("status")] ExpenseStatus Status,
    [property: JsonPropertyName("created_at")] string CreatedAt,
    [property: JsonPropertyName("currency_code")] string CurrencyCode,
    [property: JsonPropertyName("paid_on")] DateOnly PaidOn,
    [property: JsonPropertyName("gross")] decimal Gross,
    [property: JsonPropertyName("net")] decimal Net,
    [property: JsonPropertyName("attachment_ids")] IReadOnlyList<Guid> AttachmentIds,
    [property: JsonPropertyName("title")] string? Title = null,
    [property: JsonPropertyName("firstname_suffix")] string? FirstnameSuffix = null,
    [property: JsonPropertyName("lastname_company")] string? LastnameCompany = null,
    [property: JsonPropertyName("vendor")] string? Vendor = null,
    [property: JsonPropertyName("booking_account_id")] int? BookingAccountId = null,
    [property: JsonPropertyName("project_id")] Guid? ProjectId = null,
    [property: JsonPropertyName("chargeable_contact_id")] int? ChargeableContactId = null,
    [property: JsonPropertyName("transaction_id")] Guid? TransactionId = null,
    [property: JsonPropertyName("invoice_id")] Guid? InvoiceId = null
);
