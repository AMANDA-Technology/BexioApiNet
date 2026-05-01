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
/// Create view for <c>POST /4.0/payroll/employees</c>. The Bexio v4.0 spec marks
/// <c>ahvNumber</c> as required; all other fields are optional and only serialized
/// when supplied. The property name on the wire is <c>ahv_number</c>.
/// <see href="https://docs.bexio.com/#tag/Employees">Employees</see>
/// </summary>
/// <param name="AhvNumber">Swiss AHV (social security) number. Required by the API.</param>
/// <param name="Email">Employee email address.</param>
/// <param name="FirstName">Employee first name.</param>
/// <param name="LastName">Employee last name.</param>
/// <param name="PersonalNumber">Internal personal number assigned by the company.</param>
/// <param name="Nationality">ISO Alpha-2 nationality code (special values: <c>11</c> = unknown, <c>22</c> = stateless).</param>
/// <param name="Iban">Bank IBAN for salary payments.</param>
/// <param name="MaritalStatus">Marital status enum value (default <c>unknown</c>).</param>
/// <param name="Gender">Gender. Allowed values: <c>male</c>, <c>female</c>.</param>
/// <param name="DateOfBirth">Employee date of birth.</param>
/// <param name="Address">Postal address of the employee.</param>
/// <param name="Language">UI language (default <c>de</c>). Allowed values: <c>de</c>, <c>it</c>, <c>fr</c>, <c>en</c>.</param>
/// <param name="PhoneNumber">Employee phone number.</param>
/// <param name="AnnualVacationDays">Total annual vacation days entitlement.</param>
public sealed record EmployeeCreate(
    [property: JsonPropertyName("ahv_number")] string AhvNumber,
    [property: JsonPropertyName("email")] string? Email = null,
    [property: JsonPropertyName("first_name")] string? FirstName = null,
    [property: JsonPropertyName("last_name")] string? LastName = null,
    [property: JsonPropertyName("personal_number")] string? PersonalNumber = null,
    [property: JsonPropertyName("nationality")] string? Nationality = null,
    [property: JsonPropertyName("iban")] string? Iban = null,
    [property: JsonPropertyName("marital_status")] string? MaritalStatus = null,
    [property: JsonPropertyName("gender")] string? Gender = null,
    [property: JsonPropertyName("date_of_birth")] DateOnly? DateOfBirth = null,
    [property: JsonPropertyName("address")] EmployeeAddress? Address = null,
    [property: JsonPropertyName("language")] string? Language = null,
    [property: JsonPropertyName("phone_number")] string? PhoneNumber = null,
    [property: JsonPropertyName("annual_vacation_days")] int? AnnualVacationDays = null
);
