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
using BexioApiNet.Abstractions.Models.Projects.Project.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Projects;

/// <summary>
///     Live E2E coverage for <see cref="IBexioApiClient.Projects" /> exercising the full
///     <c>/2.0/pr_project</c> lifecycle (Create → Read → Update → Archive → Reactivate → Delete).
///     Tests are skipped automatically when Bexio credentials are missing via
///     <see cref="BexioE2eTestBase" />. The lifecycle test is responsible for cleaning up the
///     project it creates so the live tenant is not left with orphaned data.
/// </summary>
public sealed class ProjectServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Lists the first page of projects against the live Bexio API and asserts the response
    ///     deserializes successfully — exercises the <c>v2ListProjects</c> operation with the
    ///     <c>limit</c> query parameter.
    /// </summary>
    [Test]
    public async Task Get_ReturnsProjects()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Projects.Get(new QueryParameterProject(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Searches projects by <c>name</c> against the live Bexio API and asserts the response
    ///     deserializes successfully — exercises the <c>v2SearchProjects</c> operation.
    /// </summary>
    [Test]
    public async Task Search_ReturnsProjects()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = string.Empty, Criteria = "like" }
        };

        var result = await BexioApiClient!.Projects.Search(criteria);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Drives the full CRUD lifecycle: create a project, read it back, update its name,
    ///     archive then reactivate it, and finally delete it. Each step asserts the API result
    ///     against the OpenAPI schema (id round-trip, name update, success envelopes).
    ///     The test guards against orphan resources by deleting the project even when an
    ///     intermediate assertion fails.
    /// </summary>
    [Test]
    public async Task FullCrudLifecycle_CreateReadUpdateArchiveReactivateDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var states = await BexioApiClient!.ProjectStates.Get();
        var types = await BexioApiClient.ProjectTypes.Get();

        if (states.Data is not { Count: > 0 } stateList || types.Data is not { Count: > 0 } typeList)
        {
            Assert.Ignore("project states or types not available on this tenant");
            return;
        }

        var contacts = await BexioApiClient.Contacts.Get(new QueryParameterContact(Limit: 1));
        if (contacts.Data is not { Count: > 0 } contactList)
        {
            Assert.Ignore("no contacts available on this tenant — cannot create a project without contact_id");
            return;
        }

        var me = await BexioApiClient.Users.GetMe();
        if (me.Data is null)
        {
            Assert.Ignore("could not resolve the current user (Users.GetMe) — cannot supply user_id");
            return;
        }

        var projectName = $"E2E Project {Guid.NewGuid():N}";
        var createPayload = new ProjectCreate(
            projectName,
            contactList[0].Id,
            me.Data.Id,
            stateList[0].Id,
            typeList[0].Id);

        var createResult = await BexioApiClient.Projects.Create(createPayload);
        Assert.That(createResult.IsSuccess, Is.True, createResult.ApiError?.Message);
        Assert.That(createResult.Data, Is.Not.Null);
        var projectId = createResult.Data!.Id;

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(createResult.Data.Name, Is.EqualTo(projectName));
                Assert.That(createResult.Data.ContactId, Is.EqualTo(contactList[0].Id));
                Assert.That(createResult.Data.UserId, Is.EqualTo(me.Data!.Id));
                Assert.That(createResult.Data.PrStateId, Is.EqualTo(stateList[0].Id));
                Assert.That(createResult.Data.PrProjectTypeId, Is.EqualTo(typeList[0].Id));
            });

            var fetched = await BexioApiClient.Projects.GetById(projectId);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(projectId));
                Assert.That(fetched.Data.Name, Is.EqualTo(projectName));
            });

            var updatedName = $"{projectName} (updated)";
            var updatePayload = new ProjectUpdate(
                updatedName,
                contactList[0].Id,
                me.Data!.Id,
                stateList[0].Id,
                typeList[0].Id);

            var updateResult = await BexioApiClient.Projects.Update(projectId, updatePayload);
            Assert.Multiple(() =>
            {
                Assert.That(updateResult.IsSuccess, Is.True);
                Assert.That(updateResult.Data, Is.Not.Null);
                Assert.That(updateResult.Data!.Name, Is.EqualTo(updatedName));
            });

            var archiveResult = await BexioApiClient.Projects.Archive(projectId);
            Assert.That(archiveResult.IsSuccess, Is.True, archiveResult.ApiError?.Message);

            var reactivateResult = await BexioApiClient.Projects.Reactivate(projectId);
            Assert.That(reactivateResult.IsSuccess, Is.True, reactivateResult.ApiError?.Message);
        }
        finally
        {
            var deleteResult = await BexioApiClient.Projects.Delete(projectId);
            Assert.That(deleteResult.IsSuccess, Is.True,
                $"failed to clean up project {projectId}: {deleteResult.ApiError?.Message}");
        }
    }
}
