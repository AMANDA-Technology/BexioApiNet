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
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Expenses;

/// <summary>
/// Service for managing expenses in the Bexio expenses namespace (v4.0).
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
public interface IExpenseService
{
    /// <summary>
    /// List expenses.
    /// <see href="https://docs.bexio.com/#tag/Expenses">List Expenses</see>
    /// </summary>
    /// <param name="queryParameterExpense">Optional query parameters for pagination, sorting and filtering.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the paged <see cref="ExpenseListResponse"/> envelope.</returns>
    public Task<ApiResult<ExpenseListResponse>> Get([Optional] QueryParameterExpense? queryParameterExpense, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single expense by id.
    /// <see href="https://docs.bexio.com/#tag/Expenses">Get Expense</see>
    /// </summary>
    /// <param name="id">Expense identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the full <see cref="Expense"/>.</returns>
    public Task<ApiResult<Expense>> GetById(Guid id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Validate a proposed expense document number. Returns the next available number when
    /// the proposed one is not unique among non-draft expenses.
    /// <see href="https://docs.bexio.com/#tag/Expenses">Validate Document Number</see>
    /// </summary>
    /// <param name="documentNo">Proposed document number to validate (max 255 characters).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the <see cref="ExpenseDocumentNumberResponse"/>.</returns>
    public Task<ApiResult<ExpenseDocumentNumberResponse>> GetDocNumbers(string documentNo, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new expense.
    /// <see href="https://docs.bexio.com/#tag/Expenses">Create Expense</see>
    /// </summary>
    /// <param name="expense">Create view containing the expense details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the newly created <see cref="Expense"/>.</returns>
    public Task<ApiResult<Expense>> Create(ExpenseCreate expense, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Execute an expense action (e.g. <see cref="ExpenseAction.DUPLICATE"/>).
    /// <see href="https://docs.bexio.com/#tag/Expenses">Execute Expense Action</see>
    /// </summary>
    /// <param name="id">Expense identifier the action is executed for.</param>
    /// <param name="action">Action payload identifying the operation to perform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the resulting <see cref="Expense"/> (for <c>DUPLICATE</c> this is the new duplicate).</returns>
    public Task<ApiResult<Expense>> Actions(Guid id, ExpenseActionRequest action, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing expense. Bexio v4.0 uses PUT for full-replacement updates. When the
    /// expense is in <c>DONE</c> status only <c>attachment_ids</c> is actually persisted.
    /// <see href="https://docs.bexio.com/#tag/Expenses">Update Expense</see>
    /// </summary>
    /// <param name="id">Expense identifier to update.</param>
    /// <param name="expense">Update view containing the full expense state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="Expense"/>.</returns>
    public Task<ApiResult<Expense>> Update(Guid id, ExpenseUpdate expense, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Transition an expense's booking status between <c>DRAFT</c> and <c>DONE</c>.
    /// <see href="https://docs.bexio.com/#tag/Expenses">Update Expense Status</see>
    /// </summary>
    /// <param name="id">Expense identifier.</param>
    /// <param name="status">Target booking status (<see cref="ExpenseBookingStatus.DRAFT"/> or <see cref="ExpenseBookingStatus.DONE"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="Expense"/>.</returns>
    public Task<ApiResult<Expense>> UpdateBookings(Guid id, ExpenseBookingStatus status, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an expense. Only allowed for expenses in status <c>DRAFT</c>.
    /// <see href="https://docs.bexio.com/#tag/Expenses">Delete Expense</see>
    /// </summary>
    /// <param name="id">Expense identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> indicating success or failure.</returns>
    public Task<ApiResult<object>> Delete(Guid id, [Optional] CancellationToken cancellationToken);
}
