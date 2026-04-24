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
using BexioApiNet.Abstractions.Models.Expenses.Expenses;
using BexioApiNet.Abstractions.Models.Expenses.Expenses.Enums;
using BexioApiNet.Abstractions.Models.Expenses.Expenses.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Expenses;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Expenses;

/// <inheritdoc cref="IExpenseService" />
public sealed class ExpenseService : ConnectorService, IExpenseService
{
    /// <summary>
    /// The api endpoint version.
    /// </summary>
    private const string ApiVersion = ExpenseConfiguration.ApiVersion;

    /// <summary>
    /// The api request path for expense resources.
    /// </summary>
    private const string EndpointRoot = ExpenseConfiguration.EndpointRoot;

    /// <summary>
    /// The api request path for the document-number validation endpoint.
    /// </summary>
    private const string DocNumberEndpointRoot = ExpenseConfiguration.DocNumberEndpointRoot;

    /// <inheritdoc />
    public ExpenseService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<ExpenseListResponse>> Get([Optional] QueryParameterExpense? queryParameterExpense, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<ExpenseListResponse>($"{ApiVersion}/{EndpointRoot}", queryParameterExpense?.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Expense>> GetById(Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Expense>($"{ApiVersion}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<ExpenseDocumentNumberResponse>> GetDocNumbers(string documentNo, [Optional] CancellationToken cancellationToken)
    {
        var queryParameter = new QueryParameter(new Dictionary<string, object>
        {
            ["document_no"] = documentNo
        });

        return await ConnectionHandler.GetAsync<ExpenseDocumentNumberResponse>($"{ApiVersion}/{DocNumberEndpointRoot}", queryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Expense>> Create(ExpenseCreate expense, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Expense, ExpenseCreate>(expense, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Expense>> Actions(Guid id, ExpenseActionRequest action, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Expense, ExpenseActionRequest>(action, $"{ApiVersion}/{EndpointRoot}/{id}/actions", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Expense>> Update(Guid id, ExpenseUpdate expense, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PutAsync<Expense, ExpenseUpdate>(expense, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Expense>> UpdateBookings(Guid id, ExpenseBookingStatus status, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PutAsync<Expense, object?>(null, $"{ApiVersion}/{EndpointRoot}/{id}/bookings/{status}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}
