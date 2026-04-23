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
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Services.Connectors.Items;

namespace BexioApiNet.E2eTests.Tests.Items.StockAreas;

/// <summary>
///     Live end-to-end smoke tests for <see cref="StockAreaService" />. Instantiates the
///     service directly against a fresh <see cref="BexioConnectionHandler" /> so the tests do
///     not depend on the aggregate <c>IBexioApiClient</c> being wired up. Skipped automatically
///     when <c>BexioApiNet__BaseUri</c> or <c>BexioApiNet__JwtToken</c> are missing from the
///     environment.
/// </summary>
[Category("E2E")]
[TestFixture]
public sealed class StockAreaServiceE2eTests
{
    /// <summary>
    ///     Reads credentials from the environment and constructs the service under test.
    ///     Skips the test via <see cref="Assert.Ignore(string)" /> when credentials are absent
    ///     so CI runs without secrets still pass.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");

        if (string.IsNullOrWhiteSpace(baseUri) || string.IsNullOrWhiteSpace(jwtToken))
        {
            Assert.Ignore("credentials not configured");
            return;
        }

        _connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });
        _service = new StockAreaService(_connectionHandler);
    }

    /// <summary>
    ///     Disposes the handler if the test was not skipped.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    private BexioConnectionHandler? _connectionHandler;
    private StockAreaService? _service;

    /// <summary>
    ///     Lists all stock areas with auto-paging enabled.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsStockAreas()
    {
        Assert.That(_service, Is.Not.Null);

        var res = await _service!.Get(autoPage: true);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Posts an empty search body — Bexio treats this as "match everything" and returns
    ///     the full list of stock areas the caller is allowed to see.
    /// </summary>
    [Test]
    public async Task Search_WithEmptyCriteria_ReturnsAllStockAreas()
    {
        Assert.That(_service, Is.Not.Null);

        var res = await _service!.Search(new List<SearchCriteria>());

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }
}