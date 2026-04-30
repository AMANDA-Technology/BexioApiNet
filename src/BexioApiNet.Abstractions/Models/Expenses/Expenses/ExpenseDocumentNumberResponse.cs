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
/// Response payload of <c>GET /4.0/expenses/documentnumbers</c>. The endpoint
/// validates a proposed document number against all existing non-draft expenses and
/// returns the next available number when the proposed one is not unique.
/// <see href="https://docs.bexio.com/#tag/Expenses">Expenses</see>
/// </summary>
/// <param name="Valid">Whether the proposed document number is unique among non-draft expenses.</param>
/// <param name="NextAvailableNo">Next unique document number available to use. Set only when <see cref="Valid"/> is <see langword="false"/>.</param>
public sealed record ExpenseDocumentNumberResponse(
    [property: JsonPropertyName("valid")] bool Valid,
    [property: JsonPropertyName("next_available_no")] string? NextAvailableNo
);
