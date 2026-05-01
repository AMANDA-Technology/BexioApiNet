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
using BexioApiNet.Abstractions.Models.Payroll.Employees;
using BexioApiNet.Abstractions.Models.Payroll.Employees.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Payroll;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Payroll;

/// <inheritdoc cref="IEmployeeService" />
public sealed class EmployeeService : ConnectorService, IEmployeeService
{
    /// <summary>
    /// The api endpoint version.
    /// </summary>
    private const string ApiVersion = EmployeeConfiguration.ApiVersion;

    /// <summary>
    /// The api request path for payroll employee resources.
    /// </summary>
    private const string EndpointRoot = EmployeeConfiguration.EndpointRoot;

    /// <inheritdoc />
    public EmployeeService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<EmployeeListResponse>> Get([Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.GetAsync<EmployeeListResponse>($"{ApiVersion}/{EndpointRoot}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Employee>> GetById(Guid id, DateOnly date, [Optional] CancellationToken cancellationToken)
    {
        var queryParameter = new QueryParameterEmployee(date);
        return await ConnectionHandler.GetAsync<Employee>($"{ApiVersion}/{EndpointRoot}/{id}", queryParameter.QueryParameter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Employee>> Create(EmployeeCreate employee, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PostAsync<Employee, EmployeeCreate>(employee, $"{ApiVersion}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Employee>> Patch(Guid id, EmployeePatch employee, [Optional] CancellationToken cancellationToken)
    {
        return await ConnectionHandler.PatchAsync<Employee, EmployeePatch>(employee, $"{ApiVersion}/{EndpointRoot}/{id}", cancellationToken);
    }
}
