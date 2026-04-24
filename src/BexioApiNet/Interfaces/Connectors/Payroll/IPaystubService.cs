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

namespace BexioApiNet.Interfaces.Connectors.Payroll;

/// <summary>
///     Service for downloading payroll paystubs in the Bexio payroll namespace (v4.0).
///     Paystubs are a nested binary resource under employees — the single available
///     operation returns the raw PDF bytes for a given employee, year and month at
///     <c>/4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}</c>. No JSON
///     model is involved; the response is surfaced as <c>byte[]</c> on the
///     <see cref="ApiResult{T}" /> wrapper.
///     <see href="https://docs.bexio.com/#tag/Paystubs">Paystubs</see>
/// </summary>
public interface IPaystubService
{
    /// <summary>
    ///     Downloads the paystub PDF for an employee for a given month as raw bytes.
    ///     <see href="https://docs.bexio.com/#tag/Paystubs">Get Paystub PDF</see>
    /// </summary>
    /// <param name="employeeId">Identifier of the payroll employee.</param>
    /// <param name="year">Four-digit year of the requested paystub.</param>
    /// <param name="month">Month (1-12) of the requested paystub.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> whose <c>Data</c> contains the PDF bytes on success.</returns>
    public Task<ApiResult<byte[]>> GetPdf(int employeeId, int year, int month,
        [Optional] CancellationToken cancellationToken);
}
