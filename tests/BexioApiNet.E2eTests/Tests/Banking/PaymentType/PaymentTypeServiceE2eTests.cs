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
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.E2eTests.Tests.Banking.PaymentType;

/// <summary>
///     Live end-to-end tests for <see cref="PaymentTypeService" />. Skipped
///     automatically when <c>BexioApiNet__BaseUri</c> / <c>BexioApiNet__JwtToken</c>
///     are missing. The service is instantiated directly here because wiring the
///     connector into <see cref="IBexioApiClient" /> is deferred to the Wave 1
///     aggregation issue (#49).
/// </summary>
[Category("E2E")]
public sealed class PaymentTypeServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private IPaymentTypeService? _paymentTypeService;

    /// <summary>
    ///     Builds a dedicated <see cref="BexioConnectionHandler" /> and
    ///     <see cref="PaymentTypeService" /> per test. Calls <see cref="Assert.Ignore(string)" />
    ///     when credentials are not provided via environment variables so CI and
    ///     agent runs without a Bexio tenant do not surface false failures.
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
        _paymentTypeService = new PaymentTypeService(_connectionHandler);
    }

    /// <summary>
    ///     Disposes the connection handler if one was created for the test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    ///     <c>GET /2.0/payment_type</c> must return a non-null, non-empty list of
    ///     payment types for a provisioned Bexio tenant. Bexio ships default entries
    ///     (e.g., <c>Cash</c>, <c>Bank</c>) so the list is expected to be populated.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsPaymentTypes()
    {
        Assert.That(_paymentTypeService, Is.Not.Null);

        var result = await _paymentTypeService!.Get();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.First().Name, Is.Not.Null);
        });
    }

    /// <summary>
    ///     <c>POST /2.0/payment_type/search</c> with an <c>is_null=false</c>
    ///     <c>name</c> criterion must return a non-null list of payment types
    ///     matching the search filter.
    /// </summary>
    [Test]
    public async Task Search_WithNameCriteria_ReturnsPaymentTypes()
    {
        Assert.That(_paymentTypeService, Is.Not.Null);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = string.Empty, Criteria = "not_null" }
        };

        var result = await _paymentTypeService!.Search(criteria);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
