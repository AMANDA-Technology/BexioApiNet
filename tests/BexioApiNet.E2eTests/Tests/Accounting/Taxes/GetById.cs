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
/// Live end-to-end coverage of <c>GET /3.0/taxes/{id}</c>. Lists all taxes,
/// picks the first one and re-fetches it by id to confirm round-trip parity.
/// </summary>
public class TestGetById : BexioE2eTestBase
{
    /// <summary>
    /// Lists taxes, picks the first id, and re-fetches that tax via
    /// <c>GetById</c>. Asserts the response is successful, the id matches and the
    /// returned object satisfies the OpenAPI <c>v3Tax</c> schema (required: id, name,
    /// code, type, value, display_name).
    /// </summary>
    [Test]
    public async Task GetById()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var list = await BexioApiClient!.Taxes.Get(autoPage: true);
        Assert.That(list.IsSuccess, Is.True);
        Assert.That(list.Data, Is.Not.Null.And.Not.Empty);

        var firstId = list.Data!.First().Id;

        var res = await BexioApiClient!.Taxes.GetById(firstId);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Id, Is.EqualTo(firstId));
            Assert.That(res.Data!.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(res.Data!.Code, Is.Not.Null.And.Not.Empty);
            Assert.That(res.Data!.Type, Is.Not.Null.And.Not.Empty);
            Assert.That(res.Data!.Value, Is.GreaterThanOrEqualTo(0));
            Assert.That(res.Data!.DisplayName, Is.Not.Null.And.Not.Empty);
        });
    }
}
