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
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.E2eTests.Tests.Accounting.BusinessYears;

/// <summary>
///     Live end-to-end smoke tests for <see cref="BusinessYearService" />. Constructs the
///     service directly because the <c>BexioApiClient</c> aggregate wiring for business
///     years is tracked by a separate issue. Tests are automatically skipped when
///     <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> are not set.
/// </summary>
[Category("E2E")]
public sealed class BusinessYearServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private BusinessYearService? _service;

    /// <summary>
    ///     Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from
    ///     environment variables. Calls <see cref="Assert.Ignore(string)" /> if either
    ///     is missing so the test suite does not fail CI runs that lack live credentials.
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
        _service = new BusinessYearService(_connectionHandler);
    }

    /// <summary>
    ///     Disposes the connection handler if one was created.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    ///     Fetches the full list of business years from the live tenant and asserts
    ///     at least one entry is returned with a valid id.
    /// </summary>
    [Test]
    public async Task GetAll()
    {
        Assert.That(_service, Is.Not.Null);

        var res = await _service!.Get(autoPage: true);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.First().Id, Is.Not.Null);
        });
    }

    /// <summary>
    ///     Fetches a single business year by id (using the first one returned by the
    ///     list endpoint) and asserts round-trip equality on the id.
    /// </summary>
    [Test]
    public async Task GetById()
    {
        Assert.That(_service, Is.Not.Null);

        var list = await _service!.Get(autoPage: true);
        Assert.That(list.IsSuccess, Is.True);
        Assert.That(list.Data, Is.Not.Null.And.Not.Empty);

        var firstId = list.Data!.First().Id;
        var res = await _service.GetById(firstId);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.Id, Is.EqualTo(firstId));
        });
    }
}
