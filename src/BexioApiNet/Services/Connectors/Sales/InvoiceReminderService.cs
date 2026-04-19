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
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Sales;

/// <inheritdoc cref="IInvoiceReminderService" />
public sealed class InvoiceReminderService : ConnectorService, IInvoiceReminderService
{
    /// <summary>
    /// The api endpoint version
    /// </summary>
    private const string ApiVersion = InvoiceReminderConfiguration.ApiVersion;

    /// <summary>
    /// The api request path for the owning invoice resource
    /// </summary>
    private const string EndpointRoot = InvoiceReminderConfiguration.EndpointRoot;

    /// <summary>
    /// The nested reminder path segment
    /// </summary>
    private const string ReminderPath = InvoiceReminderConfiguration.ReminderPath;

    /// <inheritdoc />
    public InvoiceReminderService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<InvoiceReminder>?>> Get(int invoiceId, [Optional] QueryParameterInvoiceReminder? queryParameterInvoiceReminder, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<List<InvoiceReminder>?>($"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}", queryParameterInvoiceReminder?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<InvoiceReminder>> GetById(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<InvoiceReminder>($"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}/{reminderId}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<byte[]>> GetPdf(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetBinaryAsync($"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}/{reminderId}/pdf", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<InvoiceReminder>> Create(int invoiceId, InvoiceReminderCreate reminder, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<InvoiceReminder, InvoiceReminderCreate>(reminder, $"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<InvoiceReminder>>> Search(int invoiceId, List<SearchCriteria> searchCriteria, [Optional] QueryParameterInvoiceReminder? queryParameterInvoiceReminder, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostSearchAsync<InvoiceReminder>(searchCriteria, $"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}/search", queryParameterInvoiceReminder?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Send(int invoiceId, int reminderId, InvoiceReminderSendRequest request, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<object, InvoiceReminderSendRequest>(request, $"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}/{reminderId}/send", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> MarkAsSent(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}/{reminderId}/mark_as_sent", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> MarkAsUnsent(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostActionAsync($"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}/{reminderId}/mark_as_unsent", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(int invoiceId, int reminderId, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{invoiceId}/{ReminderPath}/{reminderId}", cancellationToken);
    }
}
