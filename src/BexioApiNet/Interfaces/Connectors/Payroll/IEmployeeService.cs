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
using BexioApiNet.Abstractions.Models.Payroll.Employees;
using BexioApiNet.Abstractions.Models.Payroll.Employees.Views;

namespace BexioApiNet.Interfaces.Connectors.Payroll;

/// <summary>
/// Service for managing payroll employees in the Bexio payroll namespace (v4.0).
/// Unlike expenses and bills (which use <c>PUT</c> for updates), employees are updated
/// with <c>PATCH</c> for partial updates — unspecified fields keep their current value.
/// <see href="https://docs.bexio.com/#tag/Employees">Employees</see>
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// List all payroll employees.
    /// <see href="https://docs.bexio.com/#tag/Employees">List Employees</see>
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the list of <see cref="Employee"/>.</returns>
    public Task<ApiResult<List<Employee>>> Get([Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single payroll employee by id.
    /// <see href="https://docs.bexio.com/#tag/Employees">Get Employee</see>
    /// </summary>
    /// <param name="id">Employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the <see cref="Employee"/>.</returns>
    public Task<ApiResult<Employee>> GetById(Guid id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new payroll employee.
    /// <see href="https://docs.bexio.com/#tag/Employees">Create Employee</see>
    /// </summary>
    /// <param name="employee">Create view containing the employee details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the newly created <see cref="Employee"/>.</returns>
    public Task<ApiResult<Employee>> Create(EmployeeCreate employee, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Partially update an existing payroll employee. Only the fields supplied on the
    /// <see cref="EmployeePatch"/> view are sent to Bexio and persisted — unspecified
    /// fields retain their current server-side value (<c>PATCH</c>, not <c>PUT</c>).
    /// <see href="https://docs.bexio.com/#tag/Employees">Patch Employee</see>
    /// </summary>
    /// <param name="id">Employee identifier to update.</param>
    /// <param name="employee">Patch view containing the fields to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the updated <see cref="Employee"/>.</returns>
    public Task<ApiResult<Employee>> Patch(Guid id, EmployeePatch employee, [Optional] CancellationToken cancellationToken);
}
