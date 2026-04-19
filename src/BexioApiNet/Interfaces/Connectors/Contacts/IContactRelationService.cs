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
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations;
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Contacts;

/// <summary>
/// Service for managing contact relations. <see href="https://docs.bexio.com/#tag/Contact-Relations">Contact Relations</see>
/// </summary>
public interface IContactRelationService
{
    /// <summary>
    /// Get a list of contact relations. <see href="https://docs.bexio.com/#tag/Contact-Relations/operation/v2ListContactRelations">List Contact Relations</see>
    /// </summary>
    /// <param name="queryParameterContactRelation">Query parameter specific for contact relations</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ContactRelation>?>> Get([Optional] QueryParameterContactRelation? queryParameterContactRelation, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Get a single contact relation by id. <see href="https://docs.bexio.com/#tag/Contact-Relations/operation/v2ShowContactRelation">Show Contact Relation</see>
    /// </summary>
    /// <param name="id">The contact relation id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ContactRelation>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a new contact relation. <see href="https://docs.bexio.com/#tag/Contact-Relations/operation/v2CreateContactRelation">Create Contact Relation</see>
    /// </summary>
    /// <param name="payload">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ContactRelation>> Create(ContactRelationCreate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search contact relations by one or more criteria. <see href="https://docs.bexio.com/#tag/Contact-Relations/operation/v2SearchContactRelations">Search Contact Relations</see>
    /// </summary>
    /// <param name="searchCriteria">Search criteria list sent as the JSON body</param>
    /// <param name="queryParameterContactRelation">Optional pagination / ordering query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ContactRelation>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterContactRelation? queryParameterContactRelation, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update an existing contact relation. Bexio uses HTTP POST on the resource URL for edit on v2.0 endpoints.
    /// <see href="https://docs.bexio.com/#tag/Contact-Relations/operation/v2EditContactRelation">Edit Contact Relation</see>
    /// </summary>
    /// <param name="id">The contact relation id</param>
    /// <param name="payload">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ContactRelation>> Update(int id, ContactRelationUpdate payload, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a contact relation. <see href="https://docs.bexio.com/#tag/Contact-Relations/operation/v2DeleteContactRelation">Delete Contact Relation</see>
    /// </summary>
    /// <param name="id">The contact relation id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
