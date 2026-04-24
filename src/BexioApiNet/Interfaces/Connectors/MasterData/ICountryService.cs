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
using BexioApiNet.Abstractions.Models.MasterData.Countries;
using BexioApiNet.Abstractions.Models.MasterData.Countries.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
///     Service for managing countries. <see href="https://docs.bexio.com/#tag/Countries">Countries</see>
/// </summary>
public interface ICountryService
{
    /// <summary>
    ///     Fetch a list of countries.
    ///     <see href="https://docs.bexio.com/#tag/Countries/operation/v2ListCountries">List Countries</see>
    /// </summary>
    /// <param name="queryParameterCountry">Query parameter specific for country</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Country>?>> Get([Optional] QueryParameterCountry? queryParameterCountry,
        [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Fetch a single country by id.
    ///     <see href="https://docs.bexio.com/#tag/Countries/operation/v2ShowCountry">Show Country</see>
    /// </summary>
    /// <param name="id">The country id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Country?>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Create a country.
    ///     <see href="https://docs.bexio.com/#tag/Countries/operation/v2CreateCountry">Create Country</see>
    /// </summary>
    /// <param name="country">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Country>> Create(CountryCreate country, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Search countries.
    ///     <see href="https://docs.bexio.com/#tag/Countries/operation/v2SearchCountries">Search Countries</see>
    /// </summary>
    /// <param name="searchCriteria">The search criteria list. Supported fields: <c>name</c>, <c>name_short</c>.</param>
    /// <param name="queryParameterCountry">Optional pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<Country>>> Search(List<SearchCriteria> searchCriteria,
        [Optional] QueryParameterCountry? queryParameterCountry, [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Update (edit) a country.
    ///     <see href="https://docs.bexio.com/#tag/Countries/operation/v2EditCountry">Edit Country</see>
    /// </summary>
    /// <param name="id">The country id</param>
    /// <param name="country">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<Country>> Update(int id, CountryUpdate country,
        [Optional] CancellationToken cancellationToken);

    /// <summary>
    ///     Delete a country.
    ///     <see href="https://docs.bexio.com/#tag/Countries/operation/DeleteCountry">Delete Country</see>
    /// </summary>
    /// <param name="id">The country id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
