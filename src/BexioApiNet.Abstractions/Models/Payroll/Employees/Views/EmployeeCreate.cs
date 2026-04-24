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

namespace BexioApiNet.Abstractions.Models.Payroll.Employees.Views;

/// <summary>
/// Create view for <c>POST /4.0/payroll/employees</c>. Optional fields default to
/// <see langword="null"/> and are only serialized when supplied.
/// <see href="https://docs.bexio.com/#tag/Employees">Employees</see>
/// </summary>
/// <param name="FirstName">Employee first name.</param>
/// <param name="LastName">Employee last name.</param>
/// <param name="Email">Employee email address.</param>
/// <param name="EmploymentStatus">Employment status (e.g. <c>ACTIVE</c>, <c>INACTIVE</c>).</param>
public sealed record EmployeeCreate(
    [property: JsonPropertyName("first_name")] string? FirstName = null,
    [property: JsonPropertyName("last_name")] string? LastName = null,
    [property: JsonPropertyName("email")] string? Email = null,
    [property: JsonPropertyName("employment_status")] string? EmploymentStatus = null
);
