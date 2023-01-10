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
using BexioApiNet.Abstractions.Models.Banking.BankAccounts.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Banking;

/// <summary>
/// Service for accessing bank accounts in banking namespace. <see href="https://docs.bexio.com/#tag/Bank-Accounts">Bank Accounts</see>
/// </summary>
public interface IBankAccountService
{
    /// <summary>
    /// Get a list of bank accounts. <see href="https://docs.bexio.com/#tag/Bank-Accounts/operation/ListBankAccounts">List Bank Accounts</see>
    /// </summary>
    /// <param name="queryParameterBankAccount">Query parameter specific for bank accounts</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<BankAccountGet>>> Get([Optional] QueryParameterBankAccount queryParameterBankAccount, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);
}
