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
using BexioApiNet.Abstractions.Models.Payroll.Absences;
using BexioApiNet.Abstractions.Models.Payroll.Absences.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Payroll;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Payroll;

/// <inheritdoc cref="IAbsenceService" />
public sealed class AbsenceService : ConnectorService, IAbsenceService
{
    /// <summary>
    /// The api endpoint version.
    /// </summary>
    private const string ApiVersion = AbsenceConfiguration.ApiVersion;

    /// <summary>
    /// The api request path prefix for the parent payroll employee resource.
    /// </summary>
    private const string EndpointRoot = AbsenceConfiguration.EndpointRoot;

    /// <inheritdoc />
    public AbsenceService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<AbsenceListResponse>> Get(Guid employeeId, int businessYear, [Optional] CancellationToken cancellationToken)
    {
        var queryParameter = new QueryParameterAbsence(businessYear);
        return await ConnectionHandler.GetAsync<AbsenceListResponse>($"{ApiVersion}/{EndpointRoot}/{employeeId}/absences", queryParameter.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Absence>> GetById(Guid employeeId, Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<Absence>($"{ApiVersion}/{EndpointRoot}/{employeeId}/absences/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Absence>> Create(Guid employeeId, AbsenceCreate absence, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Absence, AbsenceCreate>(absence, $"{ApiVersion}/{EndpointRoot}/{employeeId}/absences", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Absence>> Update(Guid employeeId, Guid id, AbsenceUpdate absence, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PutAsync<Absence, AbsenceUpdate>(absence, $"{ApiVersion}/{EndpointRoot}/{employeeId}/absences/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> Delete(Guid employeeId, Guid id, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/{EndpointRoot}/{employeeId}/absences/{id}", cancellationToken);
    }
}
