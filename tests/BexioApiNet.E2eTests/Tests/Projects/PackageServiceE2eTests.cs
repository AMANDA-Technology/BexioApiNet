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

using BexioApiNet.Abstractions.Models.Projects.Packages.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Projects;

/// <summary>
///     Live E2E coverage for <see cref="IBexioApiClient.Packages" /> exercising the full
///     <c>/3.0/projects/{projectId}/packages</c> lifecycle (Create → Read → Patch → Delete).
///     Tests are skipped automatically when Bexio credentials are missing via
///     <see cref="BexioE2eTestBase" />. The lifecycle test cleans up the work package it creates
///     so the live tenant is not left with orphaned data.
/// </summary>
public sealed class PackageServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Lists work packages for the first available project on the tenant and asserts the
    ///     response deserializes successfully.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsPackages()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var projects = await BexioApiClient!.Projects.Get(new QueryParameterProject(Limit: 1));
        if (projects.Data is not { Count: > 0 } projectList)
        {
            Assert.Ignore("no projects available on this tenant — cannot list work packages");
            return;
        }

        var result = await BexioApiClient.Packages.GetAsync(projectList[0].Id, new QueryParameterPackage(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Drives the full work package CRUD lifecycle: create, read by id, patch, then delete.
    ///     Each step asserts the API result against the OpenAPI <c>Work Package</c> schema.
    ///     The test guards against orphan resources by deleting the work package even when an
    ///     intermediate assertion fails.
    /// </summary>
    [Test]
    public async Task FullCrudLifecycle_CreateReadPatchDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var projects = await BexioApiClient!.Projects.Get(new QueryParameterProject(Limit: 1));
        if (projects.Data is not { Count: > 0 } projectList)
        {
            Assert.Ignore("no projects available on this tenant — cannot create a work package");
            return;
        }

        var projectId = projectList[0].Id;
        var packageName = $"E2E Work Package {Guid.NewGuid():N}";

        var createPayload = new PackageCreate(
            packageName,
            SpentTimeInHours: 0.5m,
            EstimatedTimeInHours: 1.75m,
            Comment: "Created by BexioApiNet E2E test");

        var createResult = await BexioApiClient.Packages.CreateAsync(projectId, createPayload);
        Assert.That(createResult.IsSuccess, Is.True, createResult.ApiError?.Message);
        Assert.That(createResult.Data, Is.Not.Null);

        var packageId = createResult.Data!.Id;

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(createResult.Data.Name, Is.EqualTo(packageName));
                Assert.That(createResult.Data.SpentTimeInHours, Is.EqualTo(0.5m));
                Assert.That(createResult.Data.EstimatedTimeInHours, Is.EqualTo(1.75m));
                Assert.That(createResult.Data.Comment, Is.EqualTo("Created by BexioApiNet E2E test"));
            });

            var fetched = await BexioApiClient.Packages.GetByIdAsync(projectId, packageId);
            Assert.Multiple(() =>
            {
                Assert.That(fetched.IsSuccess, Is.True);
                Assert.That(fetched.Data, Is.Not.Null);
                Assert.That(fetched.Data!.Id, Is.EqualTo(packageId));
                Assert.That(fetched.Data.Name, Is.EqualTo(packageName));
            });

            var updatedName = $"{packageName} (updated)";
            var patchResult = await BexioApiClient.Packages.PatchAsync(
                projectId,
                packageId,
                new PackagePatch(updatedName, EstimatedTimeInHours: 4m, Comment: "Updated by BexioApiNet E2E test"));

            Assert.Multiple(() =>
            {
                Assert.That(patchResult.IsSuccess, Is.True);
                Assert.That(patchResult.Data, Is.Not.Null);
                Assert.That(patchResult.Data!.Id, Is.EqualTo(packageId));
                Assert.That(patchResult.Data.Name, Is.EqualTo(updatedName));
                Assert.That(patchResult.Data.EstimatedTimeInHours, Is.EqualTo(4m));
            });
        }
        finally
        {
            var deleteResult = await BexioApiClient.Packages.DeleteAsync(projectId, packageId);
            Assert.That(deleteResult.IsSuccess, Is.True,
                $"failed to clean up work package {packageId}: {deleteResult.ApiError?.Message}");
        }
    }
}
