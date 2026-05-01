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

namespace BexioApiNet.E2eTests.Tests.Projects;

/// <summary>
///     Live E2E coverage for <c>GET /2.0/pr_project_state</c> via
///     <see cref="IBexioApiClient.ProjectStates" />. Skipped automatically when Bexio credentials
///     are missing via <see cref="BexioE2eTestBase" />.
/// </summary>
public sealed class ProjectStateServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Retrieves the project state list from the live Bexio API and asserts the response
    ///     matches the OpenAPI <c>ProjectStatus</c> schema — every returned entry must carry a
    ///     positive <c>id</c> and a non-empty <c>name</c>.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsProjectStates()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.ProjectStates.Get();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        if (result.Data is not { Count: > 0 } states)
        {
            Assert.Ignore("no project states available on this tenant");
            return;
        }

        Assert.Multiple(() =>
        {
            foreach (var state in states)
            {
                Assert.That(state.Id, Is.GreaterThan(0));
                Assert.That(state.Name, Is.Not.Null.And.Not.Empty);
            }
        });
    }
}
