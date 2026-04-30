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

namespace BexioApiNet.E2eTests.Tests.Accounting.Currencies;

/// <summary>
/// E2E coverage for <c>CurrencyService.GetById</c> against the live Bexio API.
/// </summary>
public class TestGetById : BexioE2eTestBase
{
    /// <summary>
    /// Fetches the first currency returned by <c>Get()</c> and asserts that
    /// <c>GetById</c> returns the same record. Using a discovered id keeps the
    /// test stable across tenants. Verifies the returned <c>v3CurrencyResponse</c>
    /// object matches the OpenAPI schema (id, name, round_factor are required).
    /// </summary>
    [Test]
    public async Task GetById()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var list = await BexioApiClient!.Currencies.Get();
        Assert.That(list.IsSuccess, Is.True);
        Assert.That(list.Data, Is.Not.Null.And.Not.Empty);

        var first = list.Data!.First();
        var res = await BexioApiClient!.Currencies.GetById(first.Id);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Id, Is.EqualTo(first.Id));
            Assert.That(res.Data!.Name, Is.EqualTo(first.Name));
            Assert.That(res.Data!.Name, Is.Not.Empty);
            Assert.That(res.Data!.RoundFactor, Is.GreaterThan(0));
        });
    }
}
