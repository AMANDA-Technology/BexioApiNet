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
///     Live E2E coverage placeholder for <c>GET /3.0/projects/{projectId}/packages</c>. The
///     Projects domain is not yet exposed through <see cref="IBexioApiClient" /> — wiring lands
///     with #84. Until then the test simply calls <see cref="Assert.Ignore(string)" /> so the
///     suite compiles and CI runs stay green.
/// </summary>
public sealed class PackageServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    ///     Placeholder until <c>IBexioApiClient.ProjectPackages</c> is wired (#84). Replace with
    ///     a live <c>GetAsync(projectId)</c> smoke test once the property is available.
    /// </summary>
    [Test]
    public Task GetAll()
    {
        Assert.Ignore("Projects DI wiring not yet in IBexioApiClient — see #84");
        return Task.CompletedTask;
    }
}