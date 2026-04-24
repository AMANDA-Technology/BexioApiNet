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

namespace BexioApiNet.Abstractions.Models.Expenses.Expenses;

/// <summary>
/// A discount entry on an expense. Shared by the create/update payloads and the
/// responses. On update, callers must echo an existing <c>Id</c> to preserve a
/// discount or pass <see langword="null"/> to create a new one.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="Position">Zero-based position of the discount within the expense.</param>
/// <param name="Amount">Discount amount. Max 17 digits, 2 decimals. Must be greater than 0 when booking.</param>
/// <param name="Id">Discount identifier. Server-generated on create; echoed on update.</param>
public sealed record ExpenseDiscount(
    [property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("id")] Guid? Id = null
);
