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
using BexioApiNet.Abstractions.Models.Payroll.Paystubs;

namespace BexioApiNet.Interfaces.Connectors.Payroll;

/// <summary>
///     Service for retrieving payroll paystub PDFs in the Bexio payroll namespace
///     (v4.0). Paystubs are a nested resource under employees — the single available
///     operation is routed to
///     <c>/4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}</c>. The
///     endpoint does not stream the PDF itself; it returns a JSON envelope with a
///     <see cref="Paystub.Location" /> URI that the caller must follow to download
///     the file.
///     <see href="https://docs.bexio.com/#tag/Paystubs">Paystubs</see>
/// </summary>
public interface IPaystubService
{
    /// <summary>
    ///     Retrieve the paystub download URI for an employee for a given month.
    ///     <see href="https://docs.bexio.com/#tag/Paystubs">Get Paystub PDF</see>
    /// </summary>
    /// <param name="employeeId">Identifier of the payroll employee.</param>
    /// <param name="year">Four-digit year of the requested paystub.</param>
    /// <param name="month">Month (1-12) of the requested paystub.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}" /> wrapping a <see cref="Paystub" /> whose <see cref="Paystub.Location" /> points at the generated PDF.</returns>
    public Task<ApiResult<Paystub>> GetPdf(Guid employeeId, int year, int month,
        [Optional] CancellationToken cancellationToken);
}
