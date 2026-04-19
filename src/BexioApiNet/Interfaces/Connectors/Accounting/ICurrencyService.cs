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
using BexioApiNet.Abstractions.Models.Accounting.Currencies;
using BexioApiNet.Abstractions.Models.Accounting.Currencies.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Accounting;

/// <summary>
///     Service for managing currency entries in the accounting namespace.
///     <see href="https://docs.bexio.com/#tag/Currencies">Currencies</see>
/// </summary>
public interface ICurrencyService
{
    /// <summary>
    ///     Get a list of currencies.
    ///     <see href="https://docs.bexio.com/#tag/Currencies/operation/ListCurrencies">List Currencies</see>
    /// </summary>
    /// <param name="queryParameterCurrency">Query parameter specific for currencies</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Currency>>> Get([Optional] QueryParameterCurrency? queryParameterCurrency,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch all possible currency codes (in the format CHF, EUR, etc.).
    ///     <see href="https://docs.bexio.com/#tag/Currencies/operation/ListCurrenciesCodes">List Currencies Codes</see>
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<string>>> GetCodes([Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single currency by id.
    ///     <see href="https://docs.bexio.com/#tag/Currencies/operation/ShowCurrency">Show Currency</see>
    /// </summary>
    /// <param name="id">The id of the currency</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Currency>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch all configured exchange rates for a given currency.
    ///     <see href="https://docs.bexio.com/#tag/Currencies/operation/ListExchangeRatesForCurrency">
    ///         List Exchange Rates For
    ///         Currency
    ///     </see>
    /// </summary>
    /// <param name="id">The id of the currency</param>
    /// <param name="queryParameterExchangeRate">Optional query parameter (date filter)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ExchangeRate>>> GetExchangeRates(int id,
        [Optional] QueryParameterExchangeRate? queryParameterExchangeRate,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a new currency.
    ///     <see href="https://docs.bexio.com/#tag/Currencies/operation/CreateCurrency">Create Currency</see>
    /// </summary>
    /// <param name="currency">Create view describing the new currency</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Currency>> Create(CurrencyCreate currency, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Partially update an existing currency.
    ///     <see href="https://docs.bexio.com/#tag/Currencies/operation/UpdateCurrency">Update Currency</see>
    /// </summary>
    /// <param name="id">The id of the currency to update</param>
    /// <param name="currency">Patch view describing the fields to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Currency>> Patch(int id, CurrencyPatch currency,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Permanently delete a currency by id. This cannot be undone.
    ///     <see href="https://docs.bexio.com/#tag/Currencies/operation/DeleteCurrency">Delete Currency</see>
    /// </summary>
    /// <param name="id">The id of the currency to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
