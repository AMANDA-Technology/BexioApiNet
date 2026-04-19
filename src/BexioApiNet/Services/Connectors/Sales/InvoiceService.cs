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
using BexioApiNet.Abstractions.Models.Sales.Invoices.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Sales;

/// <inheritdoc cref="IInvoiceService" />
public sealed class InvoiceService : ConnectorService, IInvoiceService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = InvoiceConfiguration.ApiVersion;

    /// <summary>
    /// The api request path
    /// </summary>
    private const string EndpointRoot = InvoiceConfiguration.EndpointRoot;

    /// <inheritdoc />
    public InvoiceService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Invoice>?>> Get([Optional] QueryParameterInvoice? queryParameterInvoice, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken)
    {
        var res = await ConnectionHandler.GetAsync<List<Invoice>?>($"{ApiVersion}/{EndpointRoot}", queryParameterInvoice?.QueryParameter, cancellationToken);

        if (!autoPage || !res.IsSuccess || res.Data is null || res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) is not { } totalResults)
            return res;

        res.Data.AddRange(await ConnectionHandler.FetchAll<Invoice>(
            res.Data.Count,
            totalResults,
            $"{ApiVersion}/{EndpointRoot}",
            queryParameterInvoice?.QueryParameter,
            cancellationToken));

        return res;
    }

    /// <inheritdoc />
    public async Task<ApiResult<Invoice>> GetById(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Invoice>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<byte[]>> GetPdf(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetBinaryAsync($"{ApiVersion}/{EndpointRoot}/{id}/pdf", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Invoice>> Create(InvoiceCreate invoice, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Invoice, InvoiceCreate>(invoice, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Invoice>> Update(int id, InvoiceUpdate invoice, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Invoice, InvoiceUpdate>(invoice, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Invoice>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterInvoice? queryParameterInvoice, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<Invoice>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/search", queryParameterInvoice?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Issue(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/issue", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> RevertIssue(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/revert_issue", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Cancel(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/cancel", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> MarkAsSent(int id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{id}/mark_as_sent", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Invoice>> Copy(int id, InvoiceCopyRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Invoice, InvoiceCopyRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/copy", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Send(int id, InvoiceSendRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<object, InvoiceSendRequest>(request, $"{ApiVersion}/{EndpointRoot}/{id}/send", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<InvoicePayment>?>> GetPayments(int invoiceId, [Optional] QueryParameterInvoice? queryParameterInvoice, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<List<InvoicePayment>?>($"{ApiVersion}/{EndpointRoot}/{invoiceId}/payment", queryParameterInvoice?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<InvoicePayment>> GetPaymentById(int invoiceId, int paymentId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<InvoicePayment>($"{ApiVersion}/{EndpointRoot}/{invoiceId}/payment/{paymentId}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<InvoicePayment>> CreatePayment(int invoiceId, InvoicePaymentCreate payment, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<InvoicePayment, InvoicePaymentCreate>(payment, $"{ApiVersion}/{EndpointRoot}/{invoiceId}/payment", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> DeletePayment(int invoiceId, int paymentId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{invoiceId}/payment/{paymentId}", cancellationToken);
    }
}
