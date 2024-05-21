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

using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Models;

namespace BexioApiNet.Tests.Tests.Accounting.ManualEntries;

/// <summary>
///
/// </summary>
public class TestGetAll : TestBase
{
    /// <summary>
    ///
    /// </summary>
    [Test]
    public async Task GetAll()
    {
        var queryParameter = new QueryParameterManualEntry(5, 0);

        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.AccountingManualEntries.Get(queryParameter, autoPage: false);
        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.Data!, Has.Count.EqualTo(5));
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.First().Id, Is.Not.Null);
        });

        var res2 = await BexioApiClient!.AccountingManualEntries.Get(queryParameter, autoPage: true);

        Assert.That(res2, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res2.Data!, Has.Count.GreaterThan(5));
            Assert.That(res2.IsSuccess, Is.True);
            Assert.That(res2.ApiError, Is.Null);
            Assert.That(res2.Data?.First().Id, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetLast50()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var res = await BexioApiClient!.AccountingManualEntries.Get(new(1, 0));
        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.Data!, Has.Count.EqualTo(1));
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.First().Id, Is.Not.Null);
        });

        var totalResults = res.ResponseHeaders?.GetValueOrDefault(ApiHeaderNames.TotalResults) ?? 0;
        Assert.That(totalResults, Is.Positive);

        var res2 = await BexioApiClient.AccountingManualEntries.Get(new(51, totalResults - 50));
        Assert.That(res2, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res2.Data!, Has.Count.EqualTo(50));
            Assert.That(res2.IsSuccess, Is.True);
            Assert.That(res2.ApiError, Is.Null);
            Assert.That(res2.Data?.First().Id, Is.Not.Null);
        });
    }
}
