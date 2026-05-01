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

namespace BexioApiNet.Abstractions.Models.Payroll.Employees;

/// <summary>
/// Envelope response returned by <c>GET /4.0/payroll/employees</c>. The v4.0 list
/// endpoint wraps results in a <c>{ data: [...] }</c> object instead of returning a
/// raw array. Items in <see cref="Data"/> only carry the base employee fields — the
/// extended computed fields (<c>annual_vacation_days_used</c>, ...) are only returned
/// by GET-by-id and POST/PATCH responses.
/// <see href="https://docs.bexio.com/#tag/Employees">Employees</see>
/// </summary>
/// <param name="Data">Active payroll employees.</param>
public sealed record EmployeeListResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<Employee> Data
);
