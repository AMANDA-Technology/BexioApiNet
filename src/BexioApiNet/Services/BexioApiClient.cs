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

using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Interfaces.Connectors.Banking;

namespace BexioApiNet.Services;

/// <inheritdoc />
public sealed class BexioApiClient : IBexioApiClient
{
    /// <summary>
    /// Private instance of connection handler used for all Services
    /// </summary>
    private readonly IBexioConnectionHandler _bexioConnectionHandler;

    /// <inheritdoc />
    public IBankAccountService BankingBankAccounts { get; set; }

    /// <inheritdoc />
    public IManualEntryService AccountingManualEntries { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioApiClient"/> class.
    /// </summary>
    public BexioApiClient(
        IBexioConnectionHandler bexioConnectionHandler,
        IBankAccountService bankingBankAccounts,
        IManualEntryService accountingManualEntries)
    {
        _bexioConnectionHandler = bexioConnectionHandler;
        BankingBankAccounts = bankingBankAccounts;
        AccountingManualEntries = accountingManualEntries;
    }
}
