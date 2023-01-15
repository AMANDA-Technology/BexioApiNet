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

using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Interfaces.Connectors.Banking;

namespace BexioApiNet.Interfaces;

/// <summary>
/// Connector service to call bexio REST API. <see href="https://docs.bexio.com/">bexio API (3.0.0)</see>
/// </summary>
public interface IBexioApiClient
{
    /// <summary>
    /// Bexio bank account connector. <see href="https://docs.bexio.com/#tag/Bank-Accounts">Bank-Accounts</see>
    /// </summary>
    public IBankAccountService BankingBankAccounts { get; set; }

    /// <summary>
    /// Bexio account connector. <see href="https://docs.bexio.com/#tag/Accounts">Accounts</see>
    /// </summary>
    public IAccountService Accounts { get; set; }

    /// <summary>
    /// Bexio currency connector. <see href="https://docs.bexio.com/#tag/Currencies">Currencies</see>
    /// </summary>
    public ICurrencyService Currencies { get; set; }

    /// <summary>
    /// Bexio account manual entry connector. <see href="https://docs.bexio.com/#tag/Manual-Entries">Manual Entries</see>
    /// </summary>
    public IManualEntryService AccountingManualEntries { get; set; }

    /// <summary>
    /// Bexio currency connector. <see href="https://docs.bexio.com/#tag/Taxes">Taxes</see>
    /// </summary>
    public ITaxService Taxes { get; set; }
}
