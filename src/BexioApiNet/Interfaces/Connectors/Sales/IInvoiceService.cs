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
using BexioApiNet.Abstractions.Models.Sales.Invoices;
using BexioApiNet.Abstractions.Models.Sales.Invoices.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Sales;

/// <summary>
/// Service for the Bexio invoice endpoints. <see href="https://docs.bexio.com/#tag/Invoices">Invoices</see>
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// List all invoices. <see href="https://docs.bexio.com/#tag/Invoices/operation/v2ListInvoices">List Invoices</see>
    /// </summary>
    /// <param name="queryParameterInvoice">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">When <see langword="true"/>, transparently pages through all remaining results via <c>X-Total-Count</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the full page (or all pages) of invoices.</returns>
    public Task<ApiResult<List<Invoice>?>> Get([Optional] QueryParameterInvoice? queryParameterInvoice, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single invoice by id. <see href="https://docs.bexio.com/#tag/Invoices/operation/v2ShowInvoice">Show Invoice</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invoice including the <c>InvoiceWithDetails</c> fields when present.</returns>
    public Task<ApiResult<Invoice>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Download the invoice as PDF. <see href="https://docs.bexio.com/#tag/Invoices/operation/v2InvoicePDF">Get Invoice PDF</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> whose <c>Data</c> is the PDF byte payload.</returns>
    public Task<ApiResult<byte[]>> GetPdf(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a single invoice. <see href="https://docs.bexio.com/#tag/Invoices/operation/v2CreateInvoice">Create Invoice</see>
    /// </summary>
    /// <param name="invoice">The invoice create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created invoice as returned by Bexio.</returns>
    public Task<ApiResult<Invoice>> Create(InvoiceCreate invoice, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Edit an existing invoice. Bexio uses <c>POST /2.0/kb_invoice/{id}</c> (not <c>PUT</c>) for
    /// full-replacement updates — see <see href="https://docs.bexio.com/#tag/Invoices/operation/v2EditInvoice">Edit Invoice</see>.
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="invoice">The invoice update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated invoice as returned by Bexio.</returns>
    public Task<ApiResult<Invoice>> Update(int id, InvoiceUpdate invoice, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an invoice. <see href="https://docs.bexio.com/#tag/Invoices/operation/v2DeleteInvoice">Delete Invoice</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search invoices by criteria. <see href="https://docs.bexio.com/#tag/Invoices/operation/v2SearchInvoices">Search Invoices</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterInvoice">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching invoices.</returns>
    public Task<ApiResult<List<Invoice>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterInvoice? queryParameterInvoice, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Issue an invoice (move from <c>Draft</c> to <c>Pending</c>).
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2IssueInvoice">Issue Invoice</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Issue(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Revert a previously issued invoice back to <c>Draft</c>.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2RevertIssueInvoice">Revert Issue Invoice</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> RevertIssue(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Cancel an invoice. <see href="https://docs.bexio.com/#tag/Invoices/operation/v2CancelInvoice">Cancel Invoice</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Cancel(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Mark an invoice as sent (without triggering an actual email dispatch).
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2MarkInvoiceAsSent">Mark Invoice as Sent</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> MarkAsSent(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Copy an existing invoice into a new draft.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2CopyInvoice">Copy Invoice</see>
    /// </summary>
    /// <param name="id">The source invoice id.</param>
    /// <param name="request">Copy request body (required <c>contact_id</c> plus optional overrides).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created invoice.</returns>
    public Task<ApiResult<Invoice>> Copy(int id, InvoiceCopyRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Send an invoice via Bexio's network mail service.
    /// <see href="https://docs.bexio.com/#tag/Invoices/operation/v2SendInvoice">Send Invoice</see>
    /// </summary>
    /// <param name="id">The invoice id.</param>
    /// <param name="request">Send request body (recipient, subject, message and optional flags).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Send(int id, InvoiceSendRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// List all payments registered against a given invoice.
    /// <see href="https://docs.bexio.com/#tag/Invoice-Payments/operation/v2ListInvoicePayments">List Invoice Payments</see>
    /// </summary>
    /// <param name="invoiceId">The invoice id.</param>
    /// <param name="queryParameterInvoice">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The payments attached to the invoice.</returns>
    public Task<ApiResult<List<InvoicePayment>?>> GetPayments(int invoiceId, [Optional] QueryParameterInvoice? queryParameterInvoice, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single invoice payment by id.
    /// <see href="https://docs.bexio.com/#tag/Invoice-Payments/operation/v2ShowInvoicePayment">Show Invoice Payment</see>
    /// </summary>
    /// <param name="invoiceId">The invoice id.</param>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invoice payment.</returns>
    public Task<ApiResult<InvoicePayment>> GetPaymentById(int invoiceId, int paymentId, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Register a payment against an invoice.
    /// <see href="https://docs.bexio.com/#tag/Invoice-Payments/operation/v2CreateInvoicePayment">Create Invoice Payment</see>
    /// </summary>
    /// <param name="invoiceId">The invoice id.</param>
    /// <param name="payment">The payment create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created invoice payment.</returns>
    public Task<ApiResult<InvoicePayment>> CreatePayment(int invoiceId, InvoicePaymentCreate payment, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a previously registered invoice payment.
    /// <see href="https://docs.bexio.com/#tag/Invoice-Payments/operation/v2DeleteInvoicePayment">Delete Invoice Payment</see>
    /// </summary>
    /// <param name="invoiceId">The invoice id.</param>
    /// <param name="paymentId">The payment id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> DeletePayment(int invoiceId, int paymentId, [Optional] CancellationToken cancellationToken);
}
