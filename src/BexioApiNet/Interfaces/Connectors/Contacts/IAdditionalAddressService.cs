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
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses;
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Contacts;

/// <summary>
/// Service for Bexio additional addresses attached to a contact. All routes are nested
/// under <c>2.0/contact/{contactId}/additional_address</c>, so every method accepts the
/// parent contact identifier. <see href="https://docs.bexio.com/#tag/Additional-Addresses">Additional Addresses</see>
/// </summary>
public interface IAdditionalAddressService
{
    /// <summary>
    /// Fetch all additional addresses for a given contact. <see href="https://docs.bexio.com/#tag/Additional-Addresses/operation/v2ListAdditionalAddresses"/>
    /// </summary>
    /// <param name="contactId">The parent contact identifier.</param>
    /// <param name="queryParameterAdditionalAddress">Optional query parameters (limit, offset).</param>
    /// <param name="autoPage">Fetch all pages using <see cref="IBexioConnectionHandler.FetchAll{T}"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API result wrapping the list of additional addresses.</returns>
    public Task<ApiResult<List<AdditionalAddress>?>> Get(int contactId, [Optional] QueryParameterAdditionalAddress? queryParameterAdditionalAddress, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single additional address by identifier for a given contact. <see href="https://docs.bexio.com/#tag/Additional-Addresses/operation/v2ShowAdditionalAddress"/>
    /// </summary>
    /// <param name="contactId">The parent contact identifier.</param>
    /// <param name="id">The additional address identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API result wrapping the additional address.</returns>
    public Task<ApiResult<AdditionalAddress>> GetById(int contactId, int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new additional address for a given contact. <see href="https://docs.bexio.com/#tag/Additional-Addresses/operation/v2CreateAdditionalAddress"/>
    /// </summary>
    /// <param name="contactId">The parent contact identifier.</param>
    /// <param name="additionalAddress">The create view payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API result wrapping the created additional address.</returns>
    public Task<ApiResult<AdditionalAddress>> Create(int contactId, AdditionalAddressCreate additionalAddress, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search additional addresses for a given contact via query criteria. Supported fields:
    /// <c>name</c>, <c>address</c>, <c>postcode</c>, <c>city</c>, <c>country_id</c>, <c>subject</c>, <c>email</c>.
    /// <see href="https://docs.bexio.com/#tag/Additional-Addresses/operation/v2SearchAdditionalAddresses"/>
    /// </summary>
    /// <param name="contactId">The parent contact identifier.</param>
    /// <param name="searchCriteria">List of search criteria entries sent as the JSON body.</param>
    /// <param name="queryParameterAdditionalAddress">Optional query parameters (limit, offset).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API result wrapping the list of matching additional addresses.</returns>
    public Task<ApiResult<List<AdditionalAddress>>> Search(int contactId, List<SearchCriteria> searchCriteria, [Optional] QueryParameterAdditionalAddress? queryParameterAdditionalAddress, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update (edit) an existing additional address. Bexio implements this as an HTTP
    /// <c>POST</c> against the single-address path (not <c>PUT</c>).
    /// <see href="https://docs.bexio.com/#tag/Additional-Addresses/operation/v2EditAdditionalAddress"/>
    /// </summary>
    /// <param name="contactId">The parent contact identifier.</param>
    /// <param name="id">The additional address identifier.</param>
    /// <param name="additionalAddress">The update view payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API result wrapping the updated additional address.</returns>
    public Task<ApiResult<AdditionalAddress>> Update(int contactId, int id, AdditionalAddressUpdate additionalAddress, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete an additional address permanently. <see href="https://docs.bexio.com/#tag/Additional-Addresses/operation/v2DeleteAdditionalAddress"/>
    /// </summary>
    /// <param name="contactId">The parent contact identifier.</param>
    /// <param name="id">The additional address identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The API result for the delete operation.</returns>
    public Task<ApiResult<object>> Delete(int contactId, int id, [Optional] CancellationToken cancellationToken);
}
