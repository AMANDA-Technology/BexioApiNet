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

namespace BexioApiNet.E2eTests.Tests.Accounting.Taxes;

/// <summary>
/// Live end-to-end coverage of <c>GET /3.0/taxes</c>. Lists all taxes for the tenant
/// and verifies the response payload matches the OpenAPI <c>v3Tax</c> schema.
/// </summary>
public class TestGetAll : BexioE2eTestBase
{
    /// <summary>
    /// Lists all taxes (across pages) and asserts both the envelope and per-item shape:
    /// <c>id</c> integer, <c>name</c> non-empty string, <c>code</c> non-empty string,
    /// <c>type</c> string in the OpenAPI enum, <c>value</c> non-negative number.
    /// </summary>
    [Test]
    public async Task GetAll()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.Taxes.Get(autoPage: true);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null.And.Not.Empty);
        });

        foreach (var tax in res.Data!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(tax.Id, Is.GreaterThan(0), "v3Tax.id must be a positive integer");
                Assert.That(tax.Name, Is.Not.Null.And.Not.Empty, "v3Tax.name is required");
                Assert.That(tax.Code, Is.Not.Null.And.Not.Empty, "v3Tax.code is required");
                Assert.That(tax.Type, Is.Not.Null.And.Not.Empty, "v3Tax.type is required");
                Assert.That(tax.Value, Is.GreaterThanOrEqualTo(0), "v3Tax.value must be non-negative");
                Assert.That(tax.DisplayName, Is.Not.Null.And.Not.Empty);
            });
        }
    }
}
