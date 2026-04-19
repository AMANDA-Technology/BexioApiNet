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
using BexioApiNet.Abstractions.Models.Contacts.ContactGroups;
using BexioApiNet.Abstractions.Models.Contacts.ContactGroups.Views;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Contacts;

/// <summary>
/// Service for managing contact groups. <see href="https://docs.bexio.com/#tag/Contact-Groups">Contact Groups</see>
/// </summary>
public interface IContactGroupService
{
    /// <summary>
    /// Fetch a list of contact groups. <see href="https://docs.bexio.com/#tag/Contact-Groups/operation/v2ListContactGroups">List Contact Groups</see>
    /// </summary>
    /// <param name="queryParameterContactGroup">Query parameter specific for contact group</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ContactGroup>?>> Get([Optional] QueryParameterContactGroup? queryParameterContactGroup, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single contact group by id. <see href="https://docs.bexio.com/#tag/Contact-Groups/operation/v2ShowContactGroup">Show Contact Group</see>
    /// </summary>
    /// <param name="id">The contact group id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ContactGroup?>> GetById(int id, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Create a contact group. <see href="https://docs.bexio.com/#tag/Contact-Groups/operation/v2CreateContactGroup">Create Contact Group</see>
    /// </summary>
    /// <param name="contactGroup">Create view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ContactGroup>> Create(ContactGroupCreate contactGroup, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search contact groups. <see href="https://docs.bexio.com/#tag/Contact-Groups/operation/v2SearchContactGroups">Search Contact Groups</see>
    /// </summary>
    /// <param name="searchCriteria">The search criteria list. Supported fields: <c>name</c>.</param>
    /// <param name="queryParameterContactGroup">Optional pagination parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ContactGroup>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterContactGroup? queryParameterContactGroup, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Update (edit) a contact group. <see href="https://docs.bexio.com/#tag/Contact-Groups/operation/v2EditContactGroup">Edit Contact Group</see>
    /// </summary>
    /// <param name="id">The contact group id</param>
    /// <param name="contactGroup">Update view</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<ContactGroup>> Update(int id, ContactGroupUpdate contactGroup, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Delete a contact group. <see href="https://docs.bexio.com/#tag/Contact-Groups/operation/v2DeleteContactGroup">Delete Contact Group</see>
    /// </summary>
    /// <param name="id">The contact group id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<object>> Delete(int id, [Optional] CancellationToken cancellationToken);
}
