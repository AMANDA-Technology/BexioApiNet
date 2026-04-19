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
using BexioApiNet.Abstractions.Models.Contacts.ContactSectors;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.Contacts;

/// <summary>
/// Service for contact sectors. Bexio exposes these under the <c>/2.0/contact_branch</c>
/// routes, but the documented concept is "contact sector".
/// <see href="https://docs.bexio.com/#tag/Contact-Sectors"/>
/// </summary>
public interface IContactSectorService
{
    /// <summary>
    /// Fetch a list of contact sectors. <see href="https://docs.bexio.com/#tag/Contact-Sectors/operation/v2ListContactSectors"/>
    /// </summary>
    /// <param name="queryParameterContactSector">Query parameters (pagination / ordering)</param>
    /// <param name="autoPage">Fetch all possible results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ContactSector>?>> Get([Optional] QueryParameterContactSector? queryParameterContactSector, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search contact sectors. <see href="https://docs.bexio.com/#tag/Contact-Sectors/operation/v2SearchContactSectors"/>
    /// Supported search fields: <c>name</c>.
    /// </summary>
    /// <param name="searchCriteria">Search criteria sent as JSON body</param>
    /// <param name="queryParameterContactSector">Optional pagination / ordering parameters appended to the URI</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    public Task<ApiResult<List<ContactSector>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterContactSector? queryParameterContactSector, [Optional] CancellationToken cancellationToken);
}
