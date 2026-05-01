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
using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Enums;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.E2eTests.Tests.Accounting.CalendarYears;

/// <summary>
/// Live end-to-end tests for <see cref="CalendarYearService"/>. Tests are skipped when
/// credentials are not configured (see <see cref="BexioE2eTestBase"/>). Constructs the
/// service directly from its own <see cref="BexioConnectionHandler"/> because the
/// aggregate <see cref="IBexioApiClient"/> wire-up lives in a separate sub-issue.
/// </summary>
public class CalendarYearServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private ICalendarYearService? _sut;

    /// <summary>
    /// Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from the
    /// environment. Calls <see cref="Assert.Ignore(string)"/> if either is missing so
    /// the test suite does not fail CI or AI agent runs that lack live credentials.
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

        _sut = new CalendarYearService(_connectionHandler);
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
    /// Fetches the paginated list of calendar years from the live tenant and asserts every
    /// returned record structurally matches the <c>CalendarYear</c> OpenAPI schema (id,
    /// start &lt;= end, vat_accounting_method ∈ {effective, net_tax}, vat_accounting_type ∈
    /// {agreed, collected}).
    /// </summary>
    [Test]
    [Category("E2E")]
    public async Task Get_ListsCalendarYears()
    {
        Assert.That(_sut, Is.Not.Null);

        var result = await _sut!.Get(new QueryParameterCalendarYear(Limit: 5, Offset: 0));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        foreach (var year in result.Data!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(year.Id, Is.GreaterThan(0), "id is required by the CalendarYear schema");
                Assert.That(year.End, Is.GreaterThanOrEqualTo(year.Start), "end must not precede start");
                Assert.That(year.VatAccountingMethod, Is.AnyOf(VatAccountingMethod.effective, VatAccountingMethod.net_tax));
                Assert.That(year.VatAccountingType, Is.AnyOf(VatAccountingType.agreed, VatAccountingType.collected));
            });
        }
    }

    /// <summary>
    /// Fetches a single calendar year by id, using the first id returned by <c>Get</c>, and
    /// asserts round-trip equality on every schema field.
    /// </summary>
    [Test]
    [Category("E2E")]
    public async Task GetById_FetchesCalendarYear()
    {
        Assert.That(_sut, Is.Not.Null);

        var list = await _sut!.Get(new QueryParameterCalendarYear(Limit: 1, Offset: 0));
        Assert.That(list.Data, Is.Not.Null.And.Not.Empty, "tenant has no calendar years to fetch");

        var listed = list.Data![0];

        var result = await _sut.GetById(listed.Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(listed.Id));
            Assert.That(result.Data!.Start, Is.EqualTo(listed.Start));
            Assert.That(result.Data!.End, Is.EqualTo(listed.End));
            Assert.That(result.Data!.IsVatSubject, Is.EqualTo(listed.IsVatSubject));
            Assert.That(result.Data!.IsAnnualReporting, Is.EqualTo(listed.IsAnnualReporting));
            Assert.That(result.Data!.VatAccountingMethod, Is.EqualTo(listed.VatAccountingMethod));
            Assert.That(result.Data!.VatAccountingType, Is.EqualTo(listed.VatAccountingType));
        });
    }

    /// <summary>
    /// Searches calendar years matching a start date criterion against the live tenant
    /// and asserts the returned record's <c>start</c> field equals the criterion value.
    /// </summary>
    [Test]
    [Category("E2E")]
    public async Task Search_FindsCalendarYearsByStart()
    {
        Assert.That(_sut, Is.Not.Null);

        var list = await _sut!.Get(new QueryParameterCalendarYear(Limit: 1, Offset: 0));
        Assert.That(list.Data, Is.Not.Null.And.Not.Empty, "tenant has no calendar years to search");

        var start = list.Data![0].Start;

        var result = await _sut.Search([
            new SearchCriteria { Field = "start", Value = start.ToString("yyyy-MM-dd"), Criteria = "=" }
        ]);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null.And.Not.Empty);
        });
        foreach (var match in result.Data!)
            Assert.That(match.Start, Is.EqualTo(start), "search results must match the supplied start criterion");
    }

    /// <summary>
    /// Creates the calendar year for a future target year. Kept as an explicit stub because
    /// Bexio persists the created years in the live tenant — this test is intentionally
    /// skipped by default (<see cref="IgnoreAttribute"/>) and should only be enabled
    /// manually when the tenant is safe to mutate.
    /// </summary>
    [Test]
    [Category("E2E")]
    [Ignore("Mutates live tenant; calendar year creation is not reversible via API. Run manually when safe.")]
    public async Task Create_CreatesCalendarYear()
    {
        Assert.That(_sut, Is.Not.Null);

        var targetYear = (DateTime.UtcNow.Year + 1).ToString();

        var result = await _sut!.Create(new(
            Year: targetYear,
            IsVatSubject: true,
            IsAnnualReporting: false,
            VatAccountingMethod: VatAccountingMethod.effective,
            VatAccountingType: VatAccountingType.agreed,
            DefaultTaxIncomeId: 1,
            DefaultTaxExpenseId: 2));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null.And.Not.Empty);
        });

        var created = result.Data!.Last();
        Assert.Multiple(() =>
        {
            Assert.That(created.Id, Is.GreaterThan(0));
            Assert.That(created.Start.Year.ToString(), Is.EqualTo(targetYear));
            Assert.That(created.IsVatSubject, Is.True);
            Assert.That(created.IsAnnualReporting, Is.False);
            Assert.That(created.VatAccountingMethod, Is.EqualTo(VatAccountingMethod.effective));
            Assert.That(created.VatAccountingType, Is.EqualTo(VatAccountingType.agreed));
        });
    }
}
