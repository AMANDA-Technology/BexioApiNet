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

using BexioApiNet.Abstractions.Models.Projects.Milestones.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Projects;

/// <summary>
///     Live E2E coverage for <see cref="IBexioApiClient.Milestones" /> exercising the full
///     <c>/3.0/projects/{projectId}/milestones</c> lifecycle (Create → Read → Update → Delete).
///     Tests are skipped automatically when Bexio credentials are missing via
///     <see cref="BexioE2eTestBase" />. The lifecycle test cleans up the milestone it creates so
///     the live tenant is not left with orphaned data.
/// </summary>
public sealed class MilestoneServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Lists milestones for the first available project on the tenant and asserts the
    ///     response deserializes successfully.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsMilestones()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var projects = await BexioApiClient!.Projects.Get(new QueryParameterProject(Limit: 1));
        if (projects.Data is not { Count: > 0 } projectList)
        {
            Assert.Ignore("no projects available on this tenant — cannot list milestones");
            return;
        }

        var result = await BexioApiClient.Milestones.GetAsync(projectList[0].Id, new QueryParameterMilestone(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Drives the full milestone CRUD lifecycle: create, read by id, update, then delete.
    ///     Each step asserts the API result against the OpenAPI <c>Milestone</c> schema.
    ///     The test guards against orphan resources by deleting the milestone even when an
    ///     intermediate assertion fails.
    /// </summary>
    [Test]
    public async Task FullCrudLifecycle_CreateReadUpdateDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var projects = await BexioApiClient!.Projects.Get(new QueryParameterProject(Limit: 1));
        if (projects.Data is not { Count: > 0 } projectList)
        {
            Assert.Ignore("no projects available on this tenant — cannot create a milestone");
            return;
        }

        var projectId = projectList[0].Id;
        var milestoneName = $"E2E Milestone {Guid.NewGuid():N}";
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var createPayload = new MilestoneCreate(
            milestoneName,
            endDate,
            "Created by BexioApiNet E2E test");

        var createResult = await BexioApiClient.Milestones.CreateAsync(projectId, createPayload);
        Assert.That(createResult.IsSuccess, Is.True, createResult.ApiError?.Message);
        Assert.That(createResult.Data, Is.Not.Null);

        var milestoneId = createResult.Data!.Id;

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(createResult.Data.Name, Is.EqualTo(milestoneName));
                Assert.That(createResult.Data.EndDate, Is.EqualTo(endDate));
                Assert.That(createResult.Data.Comment, Is.EqualTo("Created by BexioApiNet E2E test"));
            });

            var fetched = await BexioApiClient.Milestones.GetByIdAsync(projectId, milestoneId);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(milestoneId));
                Assert.That(fetched.Data.Name, Is.EqualTo(milestoneName));
            });

            var updatedName = $"{milestoneName} (updated)";
            var updateResult = await BexioApiClient.Milestones.UpdateAsync(
                projectId,
                milestoneId,
                new MilestoneUpdate(updatedName, endDate, "Updated by BexioApiNet E2E test"));

            Assert.Multiple(() =>
            {
                Assert.That(updateResult.IsSuccess, Is.True);
                Assert.That(updateResult.Data, Is.Not.Null);
                Assert.That(updateResult.Data!.Id, Is.EqualTo(milestoneId));
                Assert.That(updateResult.Data.Name, Is.EqualTo(updatedName));
            });
        }
        finally
        {
            var deleteResult = await BexioApiClient.Milestones.DeleteAsync(projectId, milestoneId);
            Assert.That(deleteResult.IsSuccess, Is.True,
                $"failed to clean up milestone {milestoneId}: {deleteResult.ApiError?.Message}");
        }
    }
}
