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
using BexioApiNet.Abstractions.Models.Contacts.Contacts;
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Contacts;

/// <summary>
/// Service for the Bexio contacts endpoints. <see href="https://docs.bexio.com/#tag/Contacts">Contacts</see>
/// </summary>
public interface IContactService
{
    /// <summary>
    /// List all contacts. <see href="https://docs.bexio.com/#tag/Contacts/operation/v2ListContacts">List Contacts</see>
    /// </summary>
    /// <param name="queryParameterContact">Optional query parameters (limit/offset/order_by/show_archived).</param>
    /// <param name="autoPage">When <see langword="true"/>, transparently pages through all remaining results via <c>X-Total-Count</c>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> with the full page (or all pages) of contacts.</returns>
    public Task<ApiResult<List<Contact>?>> Get([Optional] QueryParameterContact? queryParameterContact, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single contact by id. <see href="https://docs.bexio.com/#tag/Contacts/operation/v2ShowContact">Show Contact</see>
    /// </summary>
    /// <param name="id">The contact id.</param>
    /// <param name="queryParameterContact">Optional query parameters (only <c>show_archived</c> is meaningful on this endpoint).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contact including the <c>ContactWithDetails</c> fields when present.</returns>
    public Task<ApiResult<Contact>> GetById(int id, [Optional] QueryParameterContact? queryParameterContact, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a single contact. <see href="https://docs.bexio.com/#tag/Contacts/operation/v2CreateContact">Create Contact</see>
    /// </summary>
    /// <param name="contact">The contact create view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created contact as returned by Bexio.</returns>
    public Task<ApiResult<Contact>> Create(ContactCreate contact, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create multiple contacts in one request. <see href="https://docs.bexio.com/#tag/Contacts/operation/v2BulkCreateContacts">Bulk Create Contacts</see>
    /// </summary>
    /// <param name="contacts">The list of contact create views to submit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The contacts as returned by Bexio, in the same order as the input.</returns>
    public Task<ApiResult<List<Contact>>> BulkCreate(List<ContactCreate> contacts, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search contacts by criteria. <see href="https://docs.bexio.com/#tag/Contacts/operation/v2SearchContact">Search Contacts</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria submitted as the JSON array body.</param>
    /// <param name="queryParameterContact">Optional query parameters (limit/offset/order_by/show_archived).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching contacts.</returns>
    public Task<ApiResult<List<Contact>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterContact? queryParameterContact, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Edit an existing contact. Bexio uses <c>POST /2.0/contact/{id}</c> (not <c>PUT</c>) for
    /// full-replacement updates — see <see href="https://docs.bexio.com/#tag/Contacts/operation/v2EditContact">Edit Contact</see>.
    /// </summary>
    /// <param name="id">The contact id.</param>
    /// <param name="contact">The contact update view.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated contact as returned by Bexio.</returns>
    public Task<ApiResult<Contact>> Update(int id, ContactUpdate contact, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Restore a previously archived contact. Bexio uses <c>PATCH</c> on this endpoint —
    /// see <see href="https://docs.bexio.com/#tag/Contacts/operation/v2RestoreContact">Restore Contact</see>.
    /// </summary>
    /// <param name="id">The contact id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Restore(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete (archive) a contact. <see href="https://docs.bexio.com/#tag/Contacts/operation/v2DeleteContact">Delete Contact</see>
    /// </summary>
    /// <param name="id">The contact id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> carrying Bexio's <c>{ "success": true }</c> payload.</returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
