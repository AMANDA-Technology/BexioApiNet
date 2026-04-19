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
using BexioApiNet.Abstractions.Models.Accounting.CalendarYears;
using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Enums;
using BexioApiNet.Abstractions.Models.Accounting.CalendarYears.Views;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
///     Offline unit tests for <see cref="CalendarYearService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
///     returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class CalendarYearServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="CalendarYearService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new CalendarYearService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "3.0/accounting/calendar_years";

    private CalendarYearService _sut = null!;

    /// <summary>
    ///     Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    ///     the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<CalendarYear>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<CalendarYear>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<CalendarYear>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get forwards the <see cref="QueryParameterCalendarYear" /> to the connection handler so
    ///     limit/offset reach the server.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsQueryParameter()
    {
        var query = new QueryParameterCalendarYear(Limit: 10, Offset: 5);
        var response = new ApiResult<List<CalendarYear>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<CalendarYear>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(query);

        await ConnectionHandler.Received(1).GetAsync<List<CalendarYear>?>(
            ExpectedEndpoint,
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get (autoPage = true) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> when
    ///     the <c>X-Total-Count</c> header is present and the initial response only returned a page of
    ///     the full result set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<CalendarYear> { BuildCalendarYear(1), BuildCalendarYear(2) };
        var initial = new ApiResult<List<CalendarYear>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<CalendarYear>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<CalendarYear>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<CalendarYear>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    ///     auto-paging is not requested (no additional FetchAll round-trip, result passes through).
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<CalendarYear>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<CalendarYear>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected path
    ///     containing the id and no query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync()
    {
        const int id = 42;
        var response = new ApiResult<CalendarYear> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<CalendarYear>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        await ConnectionHandler.Received(1).GetAsync<CalendarYear>(
            $"{ExpectedEndpoint}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     GetById returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    ///     modification.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<CalendarYear> { IsSuccess = true, Data = BuildCalendarYear(1) };
        ConnectionHandler
            .GetAsync<CalendarYear>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(1);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Create forwards the payload and the expected endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<List<CalendarYear>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostAsync<List<CalendarYear>, CalendarYearCreate>(
                Arg.Any<CalendarYearCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<List<CalendarYear>, CalendarYearCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Create returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    ///     modification.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<List<CalendarYear>> { IsSuccess = true, Data = [BuildCalendarYear(1)] };
        ConnectionHandler
            .PostAsync<List<CalendarYear>, CalendarYearCreate>(
                Arg.Any<CalendarYearCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     Search forwards the criteria list, the expected <c>/search</c> endpoint and the optional
    ///     query parameter to <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsync()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "start", Value = "2026-01-01", Criteria = "=" }
        };
        var response = new ApiResult<List<CalendarYear>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<CalendarYear>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria);

        await ConnectionHandler.Received(1).PostSearchAsync<CalendarYear>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search forwards the <see cref="QueryParameterCalendarYear" /> to the connection handler so
    ///     limit/offset reach the server alongside the search body.
    /// </summary>
    [Test]
    public async Task Search_WithQueryParameter_ForwardsQueryParameter()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "start", Value = "2026-01-01", Criteria = "=" }
        };
        var query = new QueryParameterCalendarYear(Limit: 5, Offset: 0);
        var response = new ApiResult<List<CalendarYear>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<CalendarYear>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, query);

        await ConnectionHandler.Received(1).PostSearchAsync<CalendarYear>(
            criteria,
            $"{ExpectedEndpoint}/search",
            query.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    ///     modification.
    /// </summary>
    [Test]
    public async Task Search_ReturnsApiResultFromConnectionHandler()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "start", Value = "2026-01-01", Criteria = "=" }
        };
        var response = new ApiResult<List<CalendarYear>> { IsSuccess = true, Data = [BuildCalendarYear(1)] };
        ConnectionHandler
            .PostSearchAsync<CalendarYear>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Search(criteria);

        Assert.That(result, Is.SameAs(response));
    }

    private static CalendarYearCreate BuildCreatePayload()
    {
        return new CalendarYearCreate(
            Year: "2026",
            IsVatSubject: true,
            IsAnnualReporting: false,
            VatAccountingMethod: VatAccountingMethod.effective,
            VatAccountingType: VatAccountingType.agreed,
            DefaultTaxIncomeId: 1,
            DefaultTaxExpenseId: 2);
    }

    private static CalendarYear BuildCalendarYear(int id)
    {
        return new CalendarYear(
            Id: id,
            Start: new DateOnly(2026, 1, 1),
            End: new DateOnly(2026, 12, 31),
            IsVatSubject: true,
            IsAnnualReporting: false,
            CreatedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            VatAccountingMethod: VatAccountingMethod.effective,
            VatAccountingType: VatAccountingType.agreed);
    }
}
