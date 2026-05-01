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

using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Projects;

/// <summary>
///     Live E2E coverage for <c>GET /2.0/pr_project_type</c> via
///     <see cref="IBexioApiClient.ProjectTypes" />. Skipped automatically when Bexio credentials
///     are missing via <see cref="BexioE2eTestBase" />.
/// </summary>
public sealed class ProjectTypeServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Retrieves the project type list from the live Bexio API and asserts the response
    ///     matches the OpenAPI <c>ProjectType</c> schema — every returned entry must carry a
    ///     positive <c>id</c> and a non-empty <c>name</c>.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsProjectTypes()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.ProjectTypes.Get();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        if (result.Data is not { Count: > 0 } types)
        {
            Assert.Ignore("no project types available on this tenant");
            return;
        }

        Assert.Multiple(() =>
        {
            foreach (var type in types)
            {
                Assert.That(type.Id, Is.GreaterThan(0));
                Assert.That(type.Name, Is.Not.Null.And.Not.Empty);
            }
        });
    }

    /// <summary>
    ///     Retrieves the project type list ordered by <c>name</c> via the
    ///     <see cref="QueryParameterProjectType" /> wrapper — exercises the <c>order_by</c>
    ///     query parameter the OpenAPI <c>v2ListProjectType</c> operation accepts.
    /// </summary>
    [Test]
    public async Task GetAll_WithOrderByName_ReturnsProjectTypes()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.ProjectTypes.Get(new QueryParameterProjectType("name"));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
