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
using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Invoices;
using BexioApiNet.Abstractions.Models.Sales.Orders;
using BexioApiNet.Abstractions.Models.Sales.Quotes;
using BexioApiNet.Abstractions.Models.Sales.Quotes.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Sales;

/// <inheritdoc cref="IQuoteService" />
public sealed class QuoteService : ConnectorService, IQuoteService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = QuoteConfiguration.ApiVersion;

    /// <summary>
    /// The api request path
    /// </summary>
    private const string EndpointRoot = QuoteConfiguration.EndpointRoot;

    /// <inheritdoc />
    public QuoteService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Quote>?>> Get([Optional] QueryParameterQuote? queryParameterQuote, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<Quote>?>($"{ApiVersion}/{EndpointRoot}", queryParameterQuote?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null || res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<Quote>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterQuote?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<Quote>> GetById(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Quote>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<byte[]>> GetPdf(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetBinaryAsync($"{ApiVersion}/{EndpointRoot}/{id}/pdf", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Quote>> Create(QuoteCreate quote, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Quote, QuoteCreate>(quote, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Quote>> Update(int id, QuoteUpdate quote, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Quote, QuoteUpdate>(quote, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Quote>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterQuote? queryParameterQuote, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<Quote>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/search", queryParameterQuote?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Issue(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/issue", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> RevertIssue(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/revertIssue", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Accept(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/accept", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Reject(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/reject", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Reissue(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/reissue", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> MarkAsSent(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/mark_as_sent", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Send(int id, QuoteSendRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<object, QuoteSendRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/send", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Quote>> Copy(int id, QuoteCopyRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Quote, QuoteCopyRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/copy", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Order>> CreateOrderFromQuote(int id, QuoteConvertRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Order, QuoteConvertRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/order", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Invoice>> CreateInvoiceFromQuote(int id, QuoteConvertRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Invoice, QuoteConvertRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/invoice", cancellationToken);
    }
}
