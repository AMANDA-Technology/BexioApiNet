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

using System.Runtime.InteropServices;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.InvoiceReminders;
using BexioApiNet.Abstractions.Models.Sales.InvoiceReminders.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Sales;

/// <summary>
/// Service for the Bexio invoice reminder endpoints nested under
/// <c>/2.0/kb_invoice/{invoice_id}/kb_reminder</c>. All methods require the owning invoice id
/// as their first parameter because reminders are scoped to a single invoice.
/// <see href="https://docs.bexio.com/#tag/Invoices">Invoices</see>
/// </summary>
public interface IInvoiceReminderService
{
    /// <summary>
    /// List all reminders attached to a given invoice.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2ListInvoiceReminders">List Invoice Reminders</see>
    /// </summary>
    /// <param name="invoiceId">The invoice id whose reminders should be listed.</param>
    /// <param name="queryParameterInvoiceReminder">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the reminders attached to the invoice.</returns>
    public Task<ApiResult<List<InvoiceReminder>?>> Get(int invoiceId, [Optional] QueryParameterInvoiceReminder? queryParameterInvoiceReminder, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single reminder by id.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2ShowInvoiceReminder">Show Invoice Reminder</see>
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="reminderId">The reminder id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reminder.</returns>
    public Task<ApiResult<InvoiceReminder>> GetById(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Download the reminder as PDF.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2ShowInvoiceReminderPDF">Get Invoice Reminder PDF</see>
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="reminderId">The reminder id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> whose <c>Data</c> is the PDF byte payload.</returns>
    public Task<ApiResult<byte[]>> GetPdf(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a reminder for the given invoice.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2CreateInvoiceReminder">Create Invoice Reminder</see>
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="reminder">The reminder create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created reminder as returned by Bexio.</returns>
    public Task<ApiResult<InvoiceReminder>> Create(int invoiceId, InvoiceReminderCreate reminder, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search reminders by criteria.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2SearchReminders">Search Invoice Reminders</see>
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body. Supported fields: <c>title</c>, <c>reminder_level</c>, <c>is_sent</c>, <c>is_valid_from</c>, <c>is_valid_to</c>.</param>
    /// <param name="queryParameterInvoiceReminder">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching reminders.</returns>
    public Task<ApiResult<List<InvoiceReminder>>> Search(int invoiceId, List<SearchCriteria> searchCriteria, [Optional] QueryParameterInvoiceReminder? queryParameterInvoiceReminder, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Send a reminder via Bexio's network mail service.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2SendInvoiceReminder">Send Invoice Reminder</see>
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="reminderId">The reminder id.</param>
    /// <param name="request">Send request body (recipient, subject, message).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Send(int invoiceId, int reminderId, InvoiceReminderSendRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Mark a reminder as sent (without triggering an actual email dispatch).
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="reminderId">The reminder id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> MarkAsSent(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Mark a reminder as not-sent, reversing a previous <see cref="MarkAsSent"/>.
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="reminderId">The reminder id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> MarkAsUnsent(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a reminder.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2DeleteInvoiceReminder">Delete Invoice Reminder</see>
    /// </summary>
    /// <param name="invoiceId">The owning invoice id.</param>
    /// <param name="reminderId">The reminder id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken);
}
