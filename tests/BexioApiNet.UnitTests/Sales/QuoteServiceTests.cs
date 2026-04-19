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
using BexioApiNet.Abstractions.Models.Sales.Invoices;
using BexioApiNet.Abstractions.Models.Sales.Orders;
using BexioApiNet.Abstractions.Models.Sales.Quotes;
using BexioApiNet.Abstractions.Models.Sales.Quotes.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.UnitTests.Sales;

/// <summary>
/// Offline unit tests for <see cref="QuoteService" />. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
/// returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class QuoteServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="QuoteService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new QuoteService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/kb_offer";

    private QuoteService _sut = null!;

    /// <summary>
    /// Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    /// the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<Quote>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Quote>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Quote>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the <see cref="QueryParameterQuote" />'s underlying <see cref="QueryParameter" />
    /// to the connection handler so the caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterQuote(Limit: 100, Offset: 50);
        var response = new ApiResult<List<Quote>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Quote>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Quote>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get (autoPage = true) triggers <see cref="IBexioConnectionHandler.FetchAll{TResult}" /> when
    /// the <c>X-Total-Count</c> header is present and the initial response only returned a page of
    /// the full result set.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<Quote> { BuildQuote(1), BuildQuote(2) };
        var initial = new ApiResult<List<Quote>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<Quote>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<Quote>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Quote>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    /// auto-paging is not requested.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Quote>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Quote>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected
    /// endpoint path including the quote id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Quote> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Quote>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// GetPdf calls <see cref="IBexioConnectionHandler.GetBinaryAsync" /> against the
    /// <c>/{id}/pdf</c> sub-resource.
    /// </summary>
    [Test]
    public async Task GetPdf_CallsGetBinaryAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<byte[]> { IsSuccess = true, Data = [1, 2, 3] };
        string? capturedPath = null;
        ConnectionHandler
            .GetBinaryAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetPdf(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/pdf"));
    }

    /// <summary>
    /// Create forwards the payload and the expected endpoint path to
    /// <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Quote> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Quote, QuoteCreate>(
                Arg.Any<QuoteCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Quote, QuoteCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}" /> (not PUT) at
    /// <c>/2.0/kb_offer/{id}</c> — Bexio edits quotes via POST on this resource.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Quote> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Quote, QuoteUpdate>(
                Arg.Any<QuoteUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the quote id
    /// appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// Search forwards the criteria, the <c>/search</c> path and the optional query parameter to
    /// <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsync()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "title", Value = "Acme", Criteria = "like" }
        };
        var queryParameter = new QueryParameterQuote(Limit: 50);
        var response = new ApiResult<List<Quote>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<Quote>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<Quote>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Issue posts to the <c>/{id}/issue</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task Issue_CallsPostActionAsync_WithIssuePath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Issue(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/issue"));
    }

    /// <summary>
    /// RevertIssue posts to the <c>/{id}/revertIssue</c> action endpoint with no request body. Note
    /// that Bexio uses camelCase <c>revertIssue</c> on quotes (different from the snake_case used on
    /// invoices).
    /// </summary>
    [Test]
    public async Task RevertIssue_CallsPostActionAsync_WithRevertIssuePath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.RevertIssue(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/revertIssue"));
    }

    /// <summary>
    /// Accept posts to the <c>/{id}/accept</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task Accept_CallsPostActionAsync_WithAcceptPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Accept(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/accept"));
    }

    /// <summary>
    /// Reject posts to the <c>/{id}/reject</c> action endpoint with no request body. Bexio names
    /// the method "decline" but exposes it at <c>/reject</c>.
    /// </summary>
    [Test]
    public async Task Reject_CallsPostActionAsync_WithRejectPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Reject(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/reject"));
    }

    /// <summary>
    /// Reissue posts to the <c>/{id}/reissue</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task Reissue_CallsPostActionAsync_WithReissuePath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Reissue(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/reissue"));
    }

    /// <summary>
    /// MarkAsSent posts to the <c>/{id}/mark_as_sent</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task MarkAsSent_CallsPostActionAsync_WithMarkAsSentPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.MarkAsSent(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/mark_as_sent"));
    }

    /// <summary>
    /// Send posts the <see cref="QuoteSendRequest"/> body to the <c>/{id}/send</c> endpoint.
    /// </summary>
    [Test]
    public async Task Send_CallsPostAsync_WithSendPath()
    {
        const int id = 42;
        var payload = new QuoteSendRequest(
            RecipientEmail: "quote@example.com",
            Subject: "Your quote",
            Message: "Please find the document at [Network Link]");
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<object, QuoteSendRequest>(
                Arg.Any<QuoteSendRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Send(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/send"));
    }

    /// <summary>
    /// Copy posts the <see cref="QuoteCopyRequest"/> body to the <c>/{id}/copy</c> endpoint.
    /// </summary>
    [Test]
    public async Task Copy_CallsPostAsync_WithCopyPath()
    {
        const int id = 42;
        var payload = new QuoteCopyRequest(ContactId: 14);
        var response = new ApiResult<Quote> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Quote, QuoteCopyRequest>(
                Arg.Any<QuoteCopyRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Copy(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/copy"));
        await ConnectionHandler.Received(1).PostAsync<Quote, QuoteCopyRequest>(
            payload,
            $"{ExpectedEndpoint}/{id}/copy",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// CreateOrderFromQuote posts the optional <see cref="QuoteConvertRequest"/> body to the
    /// <c>/{id}/order</c> endpoint and returns the newly created <see cref="Order"/>.
    /// </summary>
    [Test]
    public async Task CreateOrderFromQuote_CallsPostAsync_WithOrderPath()
    {
        const int id = 42;
        var payload = new QuoteConvertRequest();
        var response = new ApiResult<Order> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Order, QuoteConvertRequest>(
                Arg.Any<QuoteConvertRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.CreateOrderFromQuote(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/order"));
    }

    /// <summary>
    /// CreateInvoiceFromQuote posts the optional <see cref="QuoteConvertRequest"/> body to the
    /// <c>/{id}/invoice</c> endpoint and returns the newly created <see cref="Invoice"/>.
    /// </summary>
    [Test]
    public async Task CreateInvoiceFromQuote_CallsPostAsync_WithInvoicePath()
    {
        const int id = 42;
        var payload = new QuoteConvertRequest();
        var response = new ApiResult<Invoice> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Invoice, QuoteConvertRequest>(
                Arg.Any<QuoteConvertRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.CreateInvoiceFromQuote(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/invoice"));
    }

    private static QuoteCreate BuildCreatePayload()
    {
        return new QuoteCreate(UserId: 1, Title: "Quote");
    }

    private static QuoteUpdate BuildUpdatePayload()
    {
        return new QuoteUpdate(UserId: 1, Title: "Quote");
    }

    private static Quote BuildQuote(int id)
    {
        return new Quote(
            Id: id,
            DocumentNr: $"AN-{id:D5}",
            Title: $"Quote {id}",
            ContactId: null,
            ContactSubId: null,
            UserId: 1,
            ProjectId: null,
            PrProjectId: null,
            LogopaperId: null,
            LanguageId: null,
            BankAccountId: null,
            CurrencyId: null,
            PaymentTypeId: null,
            Header: null,
            Footer: null,
            TotalGross: null,
            TotalNet: null,
            TotalTaxes: null,
            Total: null,
            TotalRoundingDifference: null,
            MwstType: null,
            MwstIsNet: null,
            ShowPositionTaxes: null,
            IsValidFrom: null,
            IsValidUntil: null,
            ContactAddress: null,
            ContactAddressManual: null,
            DeliveryAddressType: null,
            DeliveryAddress: null,
            DeliveryAddressManual: null,
            KbItemStatusId: null,
            ApiReference: null,
            ViewedByClientAt: null,
            KbTermsOfPaymentTemplateId: null,
            ShowTotal: null,
            UpdatedAt: null,
            TemplateSlug: null,
            Taxs: null,
            NetworkLink: null,
            Positions: null);
    }
}
