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
using BexioApiNet.Abstractions.Models.Banking.PaymentTypes;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Banking;

/// <summary>
/// Service for accessing payment types. <see href="https://docs.bexio.com/#tag/Payment-Types">Payment Types</see>
/// </summary>
public interface IPaymentTypeService
{
    /// <summary>
    /// Get a list of payment types.
    /// <see href="https://docs.bexio.com/#tag/Payment-Types/operation/v2ListPaymentTypes">List Payment Types</see>
    /// </summary>
    /// <param name="queryParameterPaymentType">Query parameter specific for payment types</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<PaymentType>>> Get([Optional] QueryParameterPaymentType? queryParameterPaymentType,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search payment types.
    /// <see href="https://docs.bexio.com/#tag/Payment-Types/operation/v2SearchPaymentTypes">Search Payment Types</see>
    /// </summary>
    /// <param name="searchCriteria">The search criteria sent as JSON array body. Supported field: <c>name</c>.</param>
    /// <param name="queryParameterPaymentType">Optional query parameter specific for payment types</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<PaymentType>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterPaymentType? queryParameterPaymentType,
        [Optional] CancellationToken cancellationToken);
}
