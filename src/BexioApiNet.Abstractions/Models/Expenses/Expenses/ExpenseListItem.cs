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
/// <c>GET /4.0/expenses/expenses</c>. See <see cref="Expense"/> for the full model returned
/// by the single-item endpoints.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="Id">Unique expense identifier.</param>
/// <param name="DocumentNo">Unique expense document number.</param>
/// <param name="Status">Expense status.</param>
/// <param name="SupplierId">Identifier of the supplier contact.</param>
/// <param name="ExpenseDate">Expense date.</param>
public sealed record ExpenseListItem(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("document_no")] string DocumentNo,
    [property: JsonPropertyName("status")] ExpenseStatus Status,
    [property: JsonPropertyName("supplier_id")] int SupplierId,
    [property: JsonPropertyName("expense_date")] DateOnly ExpenseDate
);
