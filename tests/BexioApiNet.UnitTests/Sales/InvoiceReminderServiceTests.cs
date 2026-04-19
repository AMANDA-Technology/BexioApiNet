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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.InvoiceReminders;
using BexioApiNet.Abstractions.Models.Sales.InvoiceReminders.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Sales;

namespace BexioApiNet.UnitTests.Sales;

/// <summary>
///     Offline unit tests for <see cref="InvoiceReminderService" />. Each test verifies that the
///     service forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected
///     arguments (including the owning invoice id in every path) and returns the handler's result
///     unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class InvoiceReminderServiceTests : ServiceTestBase
{
    /// <summary>
    ///     Creates a fresh <see cref="InvoiceReminderService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new InvoiceReminderService(ConnectionHandler);
    }

    private const int InvoiceId = 1;
    private const string ExpectedEndpoint = "2.0/kb_invoice";
    private const string ReminderPath = "kb_reminder";

    private InvoiceReminderService _sut = null!;

    /// <summary>
    ///     Get forwards the call to <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the
    ///     nested <c>/{invoiceId}/kb_reminder</c> path.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsync_WithInvoiceIdInPath()
    {
        var response = new ApiResult<List<InvoiceReminder>?> { IsSuccess = true, Data = [] };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<List<InvoiceReminder>?>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(InvoiceId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}"));
    }

    /// <summary>
    ///     GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> against
    ///     <c>/{invoiceId}/kb_reminder/{reminderId}</c>.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithReminderIdInPath()
    {
        const int reminderId = 7;
        var response = new ApiResult<InvoiceReminder> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<InvoiceReminder>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(InvoiceId, reminderId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}/{reminderId}"));
    }

    /// <summary>
    ///     GetPdf calls <see cref="IBexioConnectionHandler.GetBinaryAsync" /> against the
    ///     <c>/{reminderId}/pdf</c> sub-resource.
    /// </summary>
    [Test]
    public async Task GetPdf_CallsGetBinaryAsync_WithPdfPath()
    {
        const int reminderId = 7;
        var response = new ApiResult<byte[]> { IsSuccess = true, Data = [1, 2, 3] };
        string? capturedPath = null;
        ConnectionHandler
            .GetBinaryAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetPdf(InvoiceId, reminderId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}/{reminderId}/pdf"));
    }

    /// <summary>
    ///     Create forwards the payload and the nested endpoint path to
    ///     <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync_WithReminderPath()
    {
        var payload = new InvoiceReminderCreate(Title: "First reminder");
        var response = new ApiResult<InvoiceReminder> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<InvoiceReminder, InvoiceReminderCreate>(
                Arg.Any<InvoiceReminderCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(InvoiceId, payload);

        await ConnectionHandler.Received(1).PostAsync<InvoiceReminder, InvoiceReminderCreate>(
            payload,
            $"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}",
            Arg.Any<CancellationToken>());
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
            new() { Field = "title", Value = "First", Criteria = "like" }
        };
        var queryParameter = new QueryParameterInvoiceReminder(Limit: 50);
        var response = new ApiResult<List<InvoiceReminder>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<InvoiceReminder>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(InvoiceId, criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<InvoiceReminder>(
            criteria,
            $"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Send posts the <see cref="InvoiceReminderSendRequest"/> body to the <c>/{reminderId}/send</c>
    ///     endpoint.
    /// </summary>
    [Test]
    public async Task Send_CallsPostAsync_WithSendPath()
    {
        const int reminderId = 7;
        var payload = new InvoiceReminderSendRequest(
            RecipientEmail: "reminder@example.com",
            Subject: "Overdue invoice",
            Message: "Please find the document at [Network Link]");
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<object, InvoiceReminderSendRequest>(
                Arg.Any<InvoiceReminderSendRequest>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Send(InvoiceId, reminderId, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}/{reminderId}/send"));
    }

    /// <summary>
    ///     MarkAsSent posts to the <c>/{reminderId}/mark_as_sent</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task MarkAsSent_CallsPostActionAsync_WithMarkAsSentPath()
    {
        const int reminderId = 7;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.MarkAsSent(InvoiceId, reminderId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}/{reminderId}/mark_as_sent"));
    }

    /// <summary>
    ///     MarkAsUnsent posts to the <c>/{reminderId}/mark_as_unsent</c> action endpoint with no request body.
    /// </summary>
    [Test]
    public async Task MarkAsUnsent_CallsPostActionAsync_WithMarkAsUnsentPath()
    {
        const int reminderId = 7;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostActionAsync(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.MarkAsUnsent(InvoiceId, reminderId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}/{reminderId}/mark_as_unsent"));
    }

    /// <summary>
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the nested
    ///     <c>/{invoiceId}/kb_reminder/{reminderId}</c> path.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete_WithReminderIdInPath()
    {
        const int reminderId = 7;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(InvoiceId, reminderId);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{InvoiceId}/{ReminderPath}/{reminderId}"));
    }
}
