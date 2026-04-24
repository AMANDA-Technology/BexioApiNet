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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Projects.Milestones;
using BexioApiNet.Abstractions.Models.Projects.Milestones.Views;
using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Projects;
using BexioApiNet.Services.Connectors.Base;

namespace BexioApiNet.Services.Connectors.Projects;

/// <inheritdoc cref="IMilestoneService" />
public sealed class MilestoneService : ConnectorService, IMilestoneService
{
    /// <summary>
    ///     The api endpoint version.
    /// </summary>
    private const string ApiVersion = MilestoneConfiguration.ApiVersion;

    /// <summary>
    ///     The api request path (leaf; nested under <c>projects/{projectId}</c>).
    /// </summary>
    private const string EndpointRoot = MilestoneConfiguration.EndpointRoot;

    /// <inheritdoc />
    public MilestoneService(IBexioConnectionHandler bexioConnectionHandler) : base(bexioConnectionHandler)
    {
    }

    /// <inheritdoc />
    public async Task<ApiResult<List<Milestone>?>> GetAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await ConnectionHandler.GetAsync<List<Milestone>?>($"{ApiVersion}/projects/{projectId}/{EndpointRoot}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Milestone>> GetByIdAsync(int projectId, int id, CancellationToken cancellationToken = default)
    {
        return await ConnectionHandler.GetAsync<Milestone>($"{ApiVersion}/projects/{projectId}/{EndpointRoot}/{id}", null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Milestone>> CreateAsync(int projectId, MilestoneCreate model, CancellationToken cancellationToken = default)
    {
        return await ConnectionHandler.PostAsync<Milestone, MilestoneCreate>(model, $"{ApiVersion}/projects/{projectId}/{EndpointRoot}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<Milestone>> UpdateAsync(int projectId, int id, MilestoneUpdate model, CancellationToken cancellationToken = default)
    {
        return await ConnectionHandler.PutAsync<Milestone, MilestoneUpdate>(model, $"{ApiVersion}/projects/{projectId}/{EndpointRoot}/{id}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApiResult<object>> DeleteAsync(int projectId, int id, CancellationToken cancellationToken = default)
    {
        return await ConnectionHandler.Delete($"{ApiVersion}/projects/{projectId}/{EndpointRoot}/{id}", cancellationToken);
    }
}
