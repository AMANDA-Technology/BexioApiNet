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
using BexioApiNet.Abstractions.Models.Payroll.Absences;
using BexioApiNet.Abstractions.Models.Payroll.Absences.Views;

namespace BexioApiNet.Interfaces.Connectors.Payroll;

/// <summary>
/// Service for managing payroll absences in the Bexio payroll namespace (v4.0).
/// Absences are a nested resource under employees — every method takes the parent
/// employee identifier as its first parameter and is routed to
/// <c>/4.0/payroll/employees/{employeeId}/absences</c>. Updates use <c>PUT</c>
/// (full replacement) per the v4.0 convention shared with bills and expenses.
/// <see href="https://docs.bexio.com/#tag/Absences">Absences</see>
/// </summary>
public interface IAbsenceService
{
    /// <summary>
    /// List all absences for a given payroll employee within a business year.
    /// The Bexio v4.0 spec requires the <c>businessYear</c> query parameter.
    /// <see href="https://docs.bexio.com/#tag/Absences">List Absences</see>
    /// </summary>
    /// <param name="employeeId">Identifier of the parent employee.</param>
    /// <param name="businessYear">Four-digit business year used to filter the absences.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the <see cref="AbsenceListResponse"/> envelope.</returns>
    public Task<ApiResult<AbsenceListResponse>> Get(Guid employeeId, int businessYear, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single absence by id for a given payroll employee.
    /// <see href="https://docs.bexio.com/#tag/Absences">Get Absence</see>
    /// </summary>
    /// <param name="employeeId">Identifier of the parent employee.</param>
    /// <param name="id">Absence identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the <see cref="Absence"/>.</returns>
    public Task<ApiResult<Absence>> GetById(Guid employeeId, Guid id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new absence for a given payroll employee.
    /// <see href="https://docs.bexio.com/#tag/Absences">Create Absence</see>
    /// </summary>
    /// <param name="employeeId">Identifier of the parent employee.</param>
    /// <param name="absence">Create view containing the absence details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the newly created <see cref="Absence"/>.</returns>
    public Task<ApiResult<Absence>> Create(Guid employeeId, AbsenceCreate absence, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing absence for a given payroll employee. Bexio v4.0 uses
    /// <c>PUT</c> for full-replacement updates — all fields on
    /// <see cref="AbsenceUpdate"/> are required.
    /// <see href="https://docs.bexio.com/#tag/Absences">Update Absence</see>
    /// </summary>
    /// <param name="employeeId">Identifier of the parent employee.</param>
    /// <param name="id">Absence identifier to update.</param>
    /// <param name="absence">Update view containing the full absence state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="Absence"/>.</returns>
    public Task<ApiResult<Absence>> Update(Guid employeeId, Guid id, AbsenceUpdate absence, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an absence for a given payroll employee.
    /// <see href="https://docs.bexio.com/#tag/Absences">Delete Absence</see>
    /// </summary>
    /// <param name="employeeId">Identifier of the parent employee.</param>
    /// <param name="id">Absence identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> indicating success or failure.</returns>
    public Task<ApiResult<object>> Delete(Guid employeeId, Guid id, [Optional] CancellationToken cancellationToken);
}
