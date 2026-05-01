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
using BexioApiNet.Abstractions.Models.Accounting.VatPeriods.Enums;
using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.E2eTests.Tests.Accounting.VatPeriods;

/// <summary>
/// Live end-to-end tests for <see cref="VatPeriodService"/>. The service is not yet
/// wired into <see cref="IBexioApiClient"/> — that is the responsibility of the
/// follow-up wire-up issue — so these tests construct their own
/// <see cref="BexioConnectionHandler"/> and <see cref="VatPeriodService"/> directly
/// from the live credentials supplied via environment variables. The test is
/// skipped automatically when credentials are missing. The Bexio v3 OpenAPI spec
/// only exposes list and show operations for vat periods (no create/update/delete).
/// </summary>
[TestFixture]
public class VatPeriodServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private IVatPeriodService? _vatPeriods;

    /// <summary>
    /// Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from
    /// environment variables and wires a standalone connection handler + service
    /// for the duration of each test. Calls <see cref="Assert.Ignore(string)"/>
    /// if either credential is missing.
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

        _vatPeriods = new VatPeriodService(_connectionHandler);
    }

    /// <summary>
    /// Disposes the connection handler if it was created for the test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    /// Lists all vat periods in the test tenant. Expects a successful response
    /// with a non-null data list and asserts every entry against the OpenAPI
    /// <c>v3VatPeriod</c> schema: id, start, end, type (quarter/semester/annual),
    /// status (open/closed/closed_with_message). <c>closed_at</c> is set when the
    /// period status is closed or closed_with_message.
    /// </summary>
    [Test]
    [Category("E2E")]
    public async Task Get_ReturnsVatPeriods()
    {
        Assert.That(_vatPeriods, Is.Not.Null);

        var res = await _vatPeriods!.Get(autoPage: true);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });

        foreach (var period in res.Data!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(period.Id, Is.GreaterThan(0), "v3VatPeriod.id must be a positive integer");
                Assert.That(period.End, Is.GreaterThanOrEqualTo(period.Start), "v3VatPeriod.end must be >= start");
                Assert.That(period.Type, Is.AnyOf(VatPeriodType.quarter, VatPeriodType.semester, VatPeriodType.annual));
                Assert.That(period.Status, Is.AnyOf(VatPeriodStatus.open, VatPeriodStatus.closed, VatPeriodStatus.closed_with_message));
                if (period.Status is VatPeriodStatus.open)
                {
                    Assert.That(period.ClosedAt, Is.Null, "Open periods must not have a closed_at date");
                }
            });
        }
    }

    /// <summary>
    /// Fetches a single vat period by id using the first id returned from the
    /// list endpoint. Expects a successful response and a matching id in the
    /// payload, plus the same per-entry OpenAPI shape as the list endpoint.
    /// </summary>
    [Test]
    [Category("E2E")]
    public async Task GetById_ReturnsVatPeriod()
    {
        Assert.That(_vatPeriods, Is.Not.Null);

        var list = await _vatPeriods!.Get();
        Assert.That(list.IsSuccess, Is.True);
        Assert.That(list.Data, Is.Not.Null.And.Not.Empty);

        var firstId = list.Data!.First().Id;

        var res = await _vatPeriods.GetById(firstId);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Id, Is.EqualTo(firstId));
            Assert.That(res.Data!.End, Is.GreaterThanOrEqualTo(res.Data!.Start));
            Assert.That(res.Data!.Type, Is.AnyOf(VatPeriodType.quarter, VatPeriodType.semester, VatPeriodType.annual));
            Assert.That(res.Data!.Status, Is.AnyOf(VatPeriodStatus.open, VatPeriodStatus.closed, VatPeriodStatus.closed_with_message));
        });
    }
}
