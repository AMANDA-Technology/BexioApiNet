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

namespace BexioApiNet.E2eTests.Tests.MasterData.Permissions;

/// <summary>
///     Live E2E coverage for the Bexio v3.0 <c>/permissions</c> singleton endpoint. The
///     endpoint returns the access descriptor for the signed-in user; only a GET is
///     supported (no Create/Update/Delete in the OpenAPI spec).
/// </summary>
public sealed class TestPermissions : BexioE2eTestBase
{
    /// <summary>
    ///     Reads the permissions singleton and confirms the response carries the
    ///     <c>permissions</c> dictionary populated by Bexio. The set of keys varies per
    ///     tenant so the test only asserts the dictionary is non-empty and that each
    ///     descriptor exposes an <c>activation</c> attribute (the only attribute returned
    ///     for every resource per the Bexio docs).
    /// </summary>
    [Test]
    public async Task Get_ReturnsPermissionsForSignedInUser()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Permissions.Get();

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        Assert.That(result.Data!.Permissions, Is.Not.Null.And.Not.Empty);
        foreach (var (_, access) in result.Data.Permissions!)
            Assert.That(access.Activation, Is.Not.Null);
    }
}
