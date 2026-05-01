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
/// Payroll employee entity returned by the Bexio v4.0 <c>/payroll/employees</c>
/// endpoints (GET list, GET by id on date, POST create, PATCH update). The list
/// response only includes the base fields; GET-by-id and POST/PATCH responses also
/// include the computed <c>annual_vacation_days_used</c>,
/// <c>annual_vacation_days_left</c> and <c>effective_working_hours_per_week</c>
/// fields.
/// <see href="https://docs.bexio.com/#tag/Employees">Employees</see>
/// </summary>
/// <param name="Id">Unique employee identifier.</param>
/// <param name="Nationality">ISO Alpha-2 nationality code (special values: <c>11</c> = unknown, <c>22</c> = stateless). Required by spec.</param>
/// <param name="Language">UI language. Allowed values: <c>de</c>, <c>it</c>, <c>fr</c>, <c>en</c>. Required by spec (default <c>de</c>).</param>
/// <param name="MaritalStatus">Marital status enum value. Required by spec (default <c>unknown</c>).</param>
/// <param name="FirstName">Employee first name.</param>
/// <param name="LastName">Employee last name.</param>
/// <param name="DateOfBirth">Employee date of birth.</param>
/// <param name="AhvNumber">Swiss AHV (social security) number.</param>
/// <param name="Gender">Gender. Allowed values: <c>male</c>, <c>female</c>.</param>
/// <param name="StayPermitCategory">Swiss stay-permit category.</param>
/// <param name="Email">Employee email address.</param>
/// <param name="PhoneNumber">Employee phone number.</param>
/// <param name="HoursPerWeek">Contracted working hours per week.</param>
/// <param name="EmploymentLevel">Employment level (decimal percentage).</param>
/// <param name="AnnualVacationDaysTotal">Total annual vacation days entitlement.</param>
/// <param name="Address">Postal address of the employee.</param>
/// <param name="PersonalNumber">Internal personal number assigned by the company.</param>
/// <param name="Iban">Bank IBAN for salary payments.</param>
/// <param name="AnnualVacationDaysUsed">Vacation days used so far this year. Only present on GET-by-id and POST/PATCH responses.</param>
/// <param name="AnnualVacationDaysLeft">Vacation days remaining this year. Only present on GET-by-id and POST/PATCH responses.</param>
/// <param name="EffectiveWorkingHoursPerWeek">Effective working hours per week (post-pro-rata). Only present on GET-by-id and POST/PATCH responses.</param>
public sealed record Employee(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("nationality")] string Nationality,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("marital_status")] string MaritalStatus,
    [property: JsonPropertyName("first_name")] string? FirstName = null,
    [property: JsonPropertyName("last_name")] string? LastName = null,
    [property: JsonPropertyName("date_of_birth")] DateOnly? DateOfBirth = null,
    [property: JsonPropertyName("ahv_number")] string? AhvNumber = null,
    [property: JsonPropertyName("gender")] string? Gender = null,
    [property: JsonPropertyName("stay_permit_category")] string? StayPermitCategory = null,
    [property: JsonPropertyName("email")] string? Email = null,
    [property: JsonPropertyName("phone_number")] string? PhoneNumber = null,
    [property: JsonPropertyName("hours_per_week")] decimal? HoursPerWeek = null,
    [property: JsonPropertyName("employment_level")] decimal? EmploymentLevel = null,
    [property: JsonPropertyName("annual_vacation_days_total")] int? AnnualVacationDaysTotal = null,
    [property: JsonPropertyName("address")] EmployeeAddress? Address = null,
    [property: JsonPropertyName("personal_number")] string? PersonalNumber = null,
    [property: JsonPropertyName("iban")] string? Iban = null,
    [property: JsonPropertyName("annual_vacation_days_used")] int? AnnualVacationDaysUsed = null,
    [property: JsonPropertyName("annual_vacation_days_left")] int? AnnualVacationDaysLeft = null,
    [property: JsonPropertyName("effective_working_hours_per_week")] int? EffectiveWorkingHoursPerWeek = null
);
