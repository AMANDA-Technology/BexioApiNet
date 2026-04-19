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

using System.Net;
using BexioApiNet.Abstractions.Models.Accounting.BusinessYears;
using BexioApiNet.Abstractions.Models.Accounting.BusinessYears.Enums;
using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Accounting;

namespace BexioApiNet.UnitTests.Accounting;

/// <summary>
/// Offline unit tests for <see cref="BusinessYearService" />. Verifies the connector
/// builds the right request path, forwards cancellation tokens, and surfaces the
/// connection handler's <see cref="ApiResult{T}" /> back to the caller unchanged.
/// </summary>
[TestFixture]
public sealed class BusinessYearServiceTests : ServiceTestBase
{
    private const string ExpectedListPath = "3.0/accounting/business_years";

    /// <summary>
    /// With no parameters the service hits <c>3.0/accounting/business_years</c>
    /// exactly once with a <see langword="null" /> query parameter and returns the
    /// connection handler's <see cref="ApiResult{T}" /> as-is.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsyncOnceWithExpectedPath()
    {
        var expected = new ApiResult<List<BusinessYear>>
        {
            IsSuccess = true,
            Data = [NewBusinessYear(1)]
        };
        ConnectionHandler
            .GetAsync<List<BusinessYear>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<List<BusinessYear>>(
            ExpectedListPath,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// When a <see cref="QueryParameterBusinessYear" /> is passed, its serialized
    /// <see cref="QueryParameter" /> is forwarded to the connection handler.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterBusinessYear(20, 0);
        ConnectionHandler
            .GetAsync<List<BusinessYear>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BusinessYear>> { IsSuccess = true, Data = [] }));

        var service = new BusinessYearService(ConnectionHandler);

        await service.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<BusinessYear>>(
            ExpectedListPath,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// The cancellation token supplied by the caller must be forwarded to the
    /// connection handler so cooperative cancellation flows end-to-end.
    /// </summary>
    [Test]
    public async Task Get_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<List<BusinessYear>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<List<BusinessYear>> { IsSuccess = true, Data = [] }));

        var service = new BusinessYearService(ConnectionHandler);

        await service.Get(cancellationToken: cts.Token);

        await ConnectionHandler.Received(1).GetAsync<List<BusinessYear>>(
            ExpectedListPath,
            null,
            cts.Token);
    }

    /// <summary>
    /// A failing <see cref="ApiResult{T}" /> from the connection handler must
    /// surface to the caller untouched — the service may not swallow errors.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResultFromConnectionHandlerUnchanged()
    {
        var apiError = new ApiError(500, "boom", new object());
        var response = new ApiResult<List<BusinessYear>>
        {
            IsSuccess = false,
            ApiError = apiError,
            StatusCode = HttpStatusCode.InternalServerError,
            Data = null
        };
        ConnectionHandler
            .GetAsync<List<BusinessYear>>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(response));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// <see cref="BusinessYearService.GetById" /> must substitute the id into the
    /// path and call <c>GetAsync</c> with a null query parameter.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithIdInPath()
    {
        const int id = 42;
        var expected = new ApiResult<BusinessYear>
        {
            IsSuccess = true,
            Data = NewBusinessYear(id)
        };
        ConnectionHandler
            .GetAsync<BusinessYear>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var service = new BusinessYearService(ConnectionHandler);

        var result = await service.GetById(id);

        Assert.That(result, Is.SameAs(expected));
        await ConnectionHandler.Received(1).GetAsync<BusinessYear>(
            $"{ExpectedListPath}/{id}",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <see cref="BusinessYearService.GetById" /> forwards the caller's cancellation
    /// token to the connection handler.
    /// </summary>
    [Test]
    public async Task GetById_ForwardsCancellationTokenToConnectionHandler()
    {
        using var cts = new CancellationTokenSource();
        ConnectionHandler
            .GetAsync<BusinessYear>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new ApiResult<BusinessYear> { IsSuccess = true, Data = NewBusinessYear(1) }));

        var service = new BusinessYearService(ConnectionHandler);

        await service.GetById(7, cts.Token);

        await ConnectionHandler.Received(1).GetAsync<BusinessYear>(
            $"{ExpectedListPath}/7",
            null,
            cts.Token);
    }

    private static BusinessYear NewBusinessYear(int id)
    {
        return new BusinessYear(
            id,
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 12, 31),
            BusinessYearStatus.open,
            null);
    }
}
