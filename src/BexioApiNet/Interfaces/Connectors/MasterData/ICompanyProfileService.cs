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
using BexioApiNet.Abstractions.Models.MasterData.CompanyProfiles;

namespace BexioApiNet.Interfaces.Connectors.MasterData;

/// <summary>
/// Read-only lookup service for the Bexio company profile, exposed under
/// <c>/2.0/company_profile</c>. Each Bexio account currently owns exactly one
/// profile but the list endpoint still returns it as an array.
/// <see href="https://docs.bexio.com/#tag/Company-Profile"/>
/// </summary>
public interface ICompanyProfileService
{
    /// <summary>
    /// Fetch the list of company profiles. <see href="https://docs.bexio.com/#tag/Company-Profile/operation/v2ListCompanyProfile">List Company Profile</see>
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the list of company profiles.</returns>
    public Task<ApiResult<List<CompanyProfile>?>> Get([Optional] CancellationToken cancellationToken);

    /// <summary>
    /// Fetch a single company profile by its identifier.
    /// <see href="https://docs.bexio.com/#tag/Company-Profile/operation/v2ShowCompanyProfile">Show Company Profile</see>
    /// </summary>
    /// <param name="id">ID of the company profile to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ApiResult{T}"/> wrapping the company profile.</returns>
    public Task<ApiResult<CompanyProfile>> GetById(int id, [Optional] CancellationToken cancellationToken);
}
