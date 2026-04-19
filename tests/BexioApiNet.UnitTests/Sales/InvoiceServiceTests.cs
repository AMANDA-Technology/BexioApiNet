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
using BexioApiNet.Abstractions.Models.Sales.Invoices.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.UnitTests.Sales;

/// <summary>
///     Offline unit tests for <see cref="InvoiceService" />. Each test verifies that the service
///     forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
///     returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class InvoiceServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="InvoiceService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new InvoiceService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/kb_invoice";

    private InvoiceService _sut = null!;

    /// <summary>
    ///     Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    ///     the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<Invoice>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Invoice>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Invoice>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get forwards the <see cref="QueryParameterInvoice" />'s underlying <see cref="QueryParameter" />
    ///     to the connection handler so the caller's filters (limit/offset/order_by) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterInvoice(Limit: 100, Offset: 50);
        var response = new ApiResult<List<Invoice>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Invoice>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Invoice>?>(
            ExpectedEndpoint,
            queryParameter.QueryParameter,
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
        var initialData = new List<Invoice> { BuildInvoice(1), BuildInvoice(2) };
        var initial = new ApiResult<List<Invoice>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<Invoice>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<Invoice>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Invoice>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    ///     auto-paging is not requested.
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Invoice>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Invoice>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    ///     GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected
    ///     endpoint path including the invoice id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Invoice> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Invoice>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    ///     GetPdf calls <see cref="IBexioConnectionHandler.GetBinaryAsync" /> against the
    ///     <c>/{id}/pdf</c> sub-resource.
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
    ///     Create forwards the payload and the expected endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Invoice> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Invoice, InvoiceCreate>(
                Arg.Any<InvoiceCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Invoice, InvoiceCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}" /> (not PUT) at
    ///     <c>/2.0/kb_invoice/{id}</c> — Bexio edits invoices via POST on this resource.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Invoice> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Invoice, InvoiceUpdate>(
                Arg.Any<InvoiceUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the invoice id
    ///     appended to the endpoint root.
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
    ///     Search forwards the criteria, the <c>/search</c> path and the optional query parameter to
    ///     <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" />.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsync()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "title", Value = "Acme", Criteria = "like" }
        };
        var queryParameter = new QueryParameterInvoice(Limit: 50);
        var response = new ApiResult<List<Invoice>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<Invoice>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<Invoice>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Issue posts to the <c>/{id}/issue</c> action endpoint with no request body.
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
    ///     RevertIssue posts to the <c>/{id}/revert_issue</c> action endpoint with no request body.
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

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/revert_issue"));
    }

    /// <summary>
    ///     Cancel posts to the <c>/{id}/cancel</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task Cancel_CallsPostActionAsync_WithCancelPath()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Cancel(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/cancel"));
    }

    /// <summary>
    ///     MarkAsSent posts to the <c>/{id}/mark_as_sent</c> action endpoint with no request body.
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
    ///     Copy posts the <see cref="InvoiceCopyRequest"/> body to the <c>/{id}/copy</c> endpoint.
    /// </summary>
    [Test]
    public async Task Copy_CallsPostAsync_WithCopyPath()
    {
        const int id = 42;
        var payload = new InvoiceCopyRequest(ContactId: 14);
        var response = new ApiResult<Invoice> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Invoice, InvoiceCopyRequest>(
                Arg.Any<InvoiceCopyRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Copy(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/copy"));
        await ConnectionHandler.Received(1).PostAsync<Invoice, InvoiceCopyRequest>(
            payload,
            $"{ExpectedEndpoint}/{id}/copy",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Send posts the <see cref="InvoiceSendRequest"/> body to the <c>/{id}/send</c> endpoint.
    /// </summary>
    [Test]
    public async Task Send_CallsPostAsync_WithSendPath()
    {
        const int id = 42;
        var payload = new InvoiceSendRequest(
            RecipientEmail: "invoice@example.com",
            Subject: "Your invoice",
            Message: "Please find the document at [Network Link]");
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<object, InvoiceSendRequest>(
                Arg.Any<InvoiceSendRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Send(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/send"));
    }

    /// <summary>
    ///     GetPayments calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> against
    ///     <c>/{invoiceId}/payment</c>.
    /// </summary>
    [Test]
    public async Task GetPayments_CallsGetAsync_WithPaymentPath()
    {
        const int invoiceId = 42;
        var response = new ApiResult<List<InvoicePayment>?> { IsSuccess = true, Data = [] };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<List<InvoicePayment>?>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetPayments(invoiceId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{invoiceId}/payment"));
    }

    /// <summary>
    ///     GetPaymentById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> against
    ///     <c>/{invoiceId}/payment/{paymentId}</c>.
    /// </summary>
    [Test]
    public async Task GetPaymentById_CallsGetAsync_WithPaymentIdInPath()
    {
        const int invoiceId = 42;
        const int paymentId = 7;
        var response = new ApiResult<InvoicePayment> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<InvoicePayment>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetPaymentById(invoiceId, paymentId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{invoiceId}/payment/{paymentId}"));
    }

    /// <summary>
    ///     CreatePayment posts the <see cref="InvoicePaymentCreate"/> body to the <c>/{invoiceId}/payment</c>
    ///     endpoint.
    /// </summary>
    [Test]
    public async Task CreatePayment_CallsPostAsync_WithPaymentPath()
    {
        const int invoiceId = 42;
        var payload = new InvoicePaymentCreate(Value: "100.0000");
        var response = new ApiResult<InvoicePayment> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<InvoicePayment, InvoicePaymentCreate>(
                Arg.Any<InvoicePaymentCreate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.CreatePayment(invoiceId, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{invoiceId}/payment"));
        await ConnectionHandler.Received(1).PostAsync<InvoicePayment, InvoicePaymentCreate>(
            payload,
            $"{ExpectedEndpoint}/{invoiceId}/payment",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     DeletePayment forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the
    ///     nested <c>/payment/{paymentId}</c> path.
    /// </summary>
    [Test]
    public async Task DeletePayment_CallsConnectionHandlerDelete_WithPaymentPath()
    {
        const int invoiceId = 42;
        const int paymentId = 7;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.DeletePayment(invoiceId, paymentId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{invoiceId}/payment/{paymentId}"));
    }

    private static InvoiceCreate BuildCreatePayload()
    {
        return new InvoiceCreate(UserId: 1, Title: "Invoice");
    }

    private static InvoiceUpdate BuildUpdatePayload()
    {
        return new InvoiceUpdate(UserId: 1, Title: "Invoice");
    }

    private static Invoice BuildInvoice(int id)
    {
        return new Invoice(
            Id: id,
            DocumentNr: $"RE-{id:D5}",
            Title: $"Invoice {id}",
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
            TotalReceivedPayments: null,
            TotalCreditVouchers: null,
            TotalRemainingPayments: null,
            Total: null,
            TotalRoundingDifference: null,
            MwstType: null,
            MwstIsNet: null,
            ShowPositionTaxes: null,
            IsValidFrom: null,
            IsValidTo: null,
            ContactAddress: null,
            ContactAddressManual: null,
            KbItemStatusId: null,
            Reference: null,
            ApiReference: null,
            ViewedByClientAt: null,
            UpdatedAt: null,
            EsrId: null,
            QrInvoiceId: null,
            TemplateSlug: null,
            Taxs: null,
            NetworkLink: null,
            Positions: null);
    }
}
