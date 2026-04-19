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
using BexioApiNet.Abstractions.Models.Sales.Orders;
using BexioApiNet.Abstractions.Models.Sales.Quotes;
using BexioApiNet.Abstractions.Models.Sales.Quotes.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Sales;

/// <summary>
/// Service for the Bexio quote (offer) endpoints. <see href="https://docs.bexio.com/#tag/Quotes">Quotes</see>
/// </summary>
public interface IQuoteService
{
    /// <summary>
    /// List all quotes. <see href="https://docs.bexio.com/#tag/Quotes/operation/v2ListQuotes">List Quotes</see>
    /// </summary>
    /// <param name="queryParameterQuote">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="autoPage">When <see langword="true"/>, transparently pages through all remaining results via <c>X-Total-Count</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the full page (or all pages) of quotes.</returns>
    public Task<ApiResult<List<Quote>?>> Get([Optional] QueryParameterQuote? queryParameterQuote, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single quote by id. <see href="https://docs.bexio.com/#tag/Quotes/operation/v2ShowQuote">Show Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quote including the <c>QuoteWithDetails</c> fields when present.</returns>
    public Task<ApiResult<Quote>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Download the quote as PDF. <see href="https://docs.bexio.com/#tag/Quotes/operation/v2ShowQuotePDF">Get Quote PDF</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> whose <c>Data</c> is the PDF byte payload.</returns>
    public Task<ApiResult<byte[]>> GetPdf(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a single quote. <see href="https://docs.bexio.com/#tag/Quotes/operation/v2CreateQuote">Create Quote</see>
    /// </summary>
    /// <param name="quote">The quote create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created quote as returned by Bexio.</returns>
    public Task<ApiResult<Quote>> Create(QuoteCreate quote, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Edit an existing quote. Bexio uses <c>POST /2.0/kb_offer/{id}</c> (not <c>PUT</c>) for
    /// full-replacement updates — see <see href="https://docs.bexio.com/#tag/Quotes/operation/v2EditQuote">Edit Quote</see>.
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="quote">The quote update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated quote as returned by Bexio.</returns>
    public Task<ApiResult<Quote>> Update(int id, QuoteUpdate quote, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a quote. <see href="https://docs.bexio.com/#tag/Quotes/operation/DeleteQuote">Delete Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search quotes by criteria. <see href="https://docs.bexio.com/#tag/Quotes/operation/v2SearchQuotes">Search Quotes</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterQuote">Optional query parameters (limit/offset/order_by).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching quotes.</returns>
    public Task<ApiResult<List<Quote>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterQuote? queryParameterQuote, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Issue a quote (move from <c>Draft</c> to <c>Pending</c>).
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2IssueQuote">Issue Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Issue(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Revert a previously issued quote back to <c>Draft</c>.
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2RevertIssueQuote">Revert Issue Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> RevertIssue(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Accept a quote (move to <c>Confirmed</c>).
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2AcceptQuote">Accept Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Accept(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Reject (decline) a quote (move to <c>Declined</c>). Bexio routes this under <c>/reject</c>.
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2DeclineQuote">Decline Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Reject(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Reissue a previously accepted or declined quote back to <c>Pending</c>.
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2ReissueQuote">Reissue Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Reissue(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Mark a quote as sent (without triggering an actual email dispatch).
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2RMarkAsSentQuote">Mark Quote as Sent</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> MarkAsSent(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Send a quote via Bexio's network mail service.
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2SendQuote">Send Quote</see>
    /// </summary>
    /// <param name="id">The quote id.</param>
    /// <param name="request">Send request body (recipient, subject, message and optional flags).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Send(int id, QuoteSendRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Copy an existing quote into a new draft.
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2CopyQuote">Copy Quote</see>
    /// </summary>
    /// <param name="id">The source quote id.</param>
    /// <param name="request">Copy request body (required <c>contact_id</c> plus optional overrides).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created quote.</returns>
    public Task<ApiResult<Quote>> Copy(int id, QuoteCopyRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create an order from an existing quote.
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2CreateOrderFromQuote">Create Order From Quote</see>
    /// </summary>
    /// <param name="id">The source quote id.</param>
    /// <param name="request">Optional subset of positions to carry over. When omitted, all positions are copied.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created order.</returns>
    public Task<ApiResult<Order>> CreateOrderFromQuote(int id, QuoteConvertRequest request, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create an invoice from an existing quote.
    /// <see href="https://docs.bexio.com/#tag/Quotes/operation/v2CreateInvoiceFromQuote">Create Invoice From Quote</see>
    /// </summary>
    /// <param name="id">The source quote id.</param>
    /// <param name="request">Optional subset of positions to carry over. When omitted, all positions are copied.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created invoice.</returns>
    public Task<ApiResult<Invoice>> CreateInvoiceFromQuote(int id, QuoteConvertRequest request, [Optional] CancellationToken cancellationToken);
}
