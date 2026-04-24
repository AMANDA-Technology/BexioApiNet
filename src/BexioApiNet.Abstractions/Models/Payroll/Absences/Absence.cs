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

namespace BexioApiNet.Abstractions.Models.Payroll.Absences;

/// <summary>
/// Payroll absence entity returned by the Bexio v4.0
/// <c>/payroll/employees/{employeeId}/absences</c> endpoints (GET list, GET by id,
/// POST create, PUT update). Absences are nested resources scoped to a single employee.
/// <see href="https://docs.bexio.com/#tag/Absences">Absences</see>
/// </summary>
/// <param name="Id">Unique absence identifier.</param>
/// <param name="EmployeeId">Identifier of the employee the absence belongs to.</param>
/// <param name="AbsenceType">Absence type (e.g. <c>SICK</c>, <c>VACATION</c>, <c>MATERNITY</c>).</param>
/// <param name="StartDate">First day of the absence.</param>
/// <param name="EndDate">Last day of the absence.</param>
/// <param name="Status">Approval status (e.g. <c>PENDING</c>, <c>APPROVED</c>, <c>REJECTED</c>).</param>
/// <param name="CreatedAt">Creation timestamp.</param>
public sealed record Absence(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("employee_id")] Guid? EmployeeId = null,
    [property: JsonPropertyName("absence_type")] string? AbsenceType = null,
    [property: JsonPropertyName("start_date")] DateTime? StartDate = null,
    [property: JsonPropertyName("end_date")] DateTime? EndDate = null,
    [property: JsonPropertyName("status")] string? Status = null,
    [property: JsonPropertyName("created_at")] DateTime? CreatedAt = null
);
