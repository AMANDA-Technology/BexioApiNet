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
using BexioApiNet.Abstractions.Models.MasterData.CommunicationTypes;
using BexioApiNet.Models;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
/// Read-only lookup service for Bexio communication types. Bexio exposes the resource under
/// the legacy path segment <c>/2.0/communication_kind</c> while the documented concept is
/// "communication type".
/// <see href="https://docs.bexio.com/#tag/Communication-Types"/>
/// </summary>
public interface ICommunicationTypeService
{
    /// <summary>
    /// Fetch a list of communication types. <see href="https://docs.bexio.com/#tag/Communication-Types/operation/v2ListCommunicationTypes">List Communication Types</see>
    /// </summary>
    /// <param name="queryParameterCommunicationType">Query parameters (pagination / ordering).</param>
    /// <param name="autoPage">Fetch all possible results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the list of communication types.</returns>
    public Task<ApiResult<List<CommunicationType>?>> Get([Optional] QueryParameterCommunicationType? queryParameterCommunicationType, [Optional] bool autoPage, [Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Search communication types. <see href="https://docs.bexio.com/#tag/Communication-Types/operation/v2SearchCommunicationTypes">Search Communication Types</see>
    /// Supported search fields: <c>name</c>.
    /// </summary>
    /// <param name="searchCriteria">Search criteria sent as JSON body.</param>
    /// <param name="queryParameterCommunicationType">Optional pagination / ordering parameters appended to the URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the list of matched communication types.</returns>
    public Task<ApiResult<List<CommunicationType>>> Search(List<SearchCriteria> searchCriteria, [Optional] QueryParameterCommunicationType? queryParameterCommunicationType, [Optional] CancellationToken cancellationToken);
}
