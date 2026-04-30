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
using BexioApiNet.Abstractions.Models.Contacts.Contacts;
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.UnitTests.Contacts;

/// <summary>
/// Offline unit tests for <see cref="ContactService" />. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
/// returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class ContactServiceTests : ServiceTestBase
{
    /// <summary>
    /// Creates a fresh <see cref="ContactService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ContactService(ConnectionHandler);
    }

    private const string ExpectedEndpoint = "2.0/contact";

    private ContactService _sut = null!;

    /// <summary>
    /// Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    /// the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<Contact>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Contact>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<Contact>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the <see cref="QueryParameterContact" />'s underlying <see cref="QueryParameter" />
    /// to the connection handler so the caller's filters (limit/offset/order_by/show_archived) reach the API.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_PassesQueryParameterToConnectionHandler()
    {
        var queryParameter = new QueryParameterContact(Limit: 100, Offset: 50);
        var response = new ApiResult<List<Contact>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Contact>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<Contact>?>(
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
        var initialData = new List<Contact> { BuildContact(1), BuildContact(2) };
        var initial = new ApiResult<List<Contact>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<Contact>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<Contact>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<Contact>(
            initialData.Count,
            totalResults,
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get returns the <see cref="ApiResult{T}" /> produced by the connection handler when
    /// auto-paging is not requested (no additional FetchAll round-trip, result passes through).
    /// </summary>
    [Test]
    public async Task Get_ReturnsApiResult()
    {
        var response = new ApiResult<List<Contact>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<Contact>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with the expected
    /// endpoint path including the contact id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsync_WithIdInPath()
    {
        const int id = 42;
        var response = new ApiResult<Contact> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<Contact>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<Contact> { IsSuccess = true };
        ConnectionHandler
            .GetAsync<Contact>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(1);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Create forwards the payload and the expected endpoint path to
    /// <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsync()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Contact> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Contact, ContactCreate>(
                Arg.Any<ContactCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<Contact, ContactCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}" /> produced by the connection handler without modification.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<Contact> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<Contact, ContactCreate>(
                Arg.Any<ContactCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// BulkCreate forwards the list of payloads and the <c>/_bulk_create</c> path to
    /// <see cref="IBexioConnectionHandler.PostBulkAsync{TResult,TCreate}" />.
    /// </summary>
    [Test]
    public async Task BulkCreate_CallsPostBulkAsync()
    {
        var payloads = new List<ContactCreate> { BuildCreatePayload(), BuildCreatePayload() };
        var response = new ApiResult<List<Contact>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostBulkAsync<Contact, ContactCreate>(
                Arg.Any<List<ContactCreate>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.BulkCreate(payloads);

        await ConnectionHandler.Received(1).PostBulkAsync<Contact, ContactCreate>(
            payloads,
            $"{ExpectedEndpoint}/_bulk_create",
            Arg.Any<CancellationToken>());
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
            new() { Field = "name_1", Value = "Acme", Criteria = "like" }
        };
        var queryParameter = new QueryParameterContact(Limit: 50);
        var response = new ApiResult<List<Contact>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<Contact>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<Contact>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update calls <see cref="IBexioConnectionHandler.PostAsync{TResult,TUpdate}" /> (not PUT) at
    /// <c>/2.0/contact/{id}</c> — Bexio edits contacts via POST on this resource.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 42;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Contact> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<Contact, ContactUpdate>(
                Arg.Any<ContactUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
        await ConnectionHandler.Received(1).PostAsync<Contact, ContactUpdate>(
            payload,
            $"{ExpectedEndpoint}/{id}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Restore calls <see cref="IBexioConnectionHandler.PatchAsync{TResult,TPatch}" /> at
    /// <c>/2.0/contact/{id}/restore</c> with a <see langword="null" /> payload (no request body).
    /// </summary>
    [Test]
    public async Task Restore_CallsPatchAsync_WithRestoreInPath_AndNullPayload()
    {
        const int id = 42;
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PatchAsync<object, object?>(
                Arg.Any<object?>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Restore(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}/restore"));
        await ConnectionHandler.Received(1).PatchAsync<object, object?>(
            null,
            $"{ExpectedEndpoint}/{id}/restore",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with the contact id
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

    private static ContactCreate BuildCreatePayload()
    {
        return new ContactCreate(
            ContactTypeId: 1,
            Name1: "Acme AG",
            UserId: 1,
            OwnerId: 1);
    }

    private static ContactUpdate BuildUpdatePayload()
    {
        return new ContactUpdate(
            ContactTypeId: 1,
            Name1: "Acme AG",
            UserId: 1,
            OwnerId: 1);
    }

    private static Contact BuildContact(int id)
    {
        return new Contact(
            Id: id,
            Nr: null,
            ContactTypeId: 1,
            Name1: $"Contact {id}",
            Name2: null,
            SalutationId: null,
            SalutationForm: null,
            TitleId: null,
            Birthday: null,
            Address: null,
            StreetName: null,
            HouseNumber: null,
            AddressAddition: null,
            Postcode: null,
            City: null,
            CountryId: null,
            Mail: null,
            MailSecond: null,
            PhoneFixed: null,
            PhoneFixedSecond: null,
            PhoneMobile: null,
            Fax: null,
            Url: null,
            SkypeName: null,
            Remarks: null,
            LanguageId: null,
            IsLead: null,
            ContactGroupIds: null,
            ContactBranchIds: null,
            UserId: 1,
            OwnerId: 1,
            UpdatedAt: null,
            ProfileImage: null);
    }

    /// <summary>
    /// <see cref="QueryParameterContact"/> emits all four spec-defined parameters
    /// (<c>limit</c>, <c>offset</c>, <c>order_by</c>, <c>show_archived</c>) when populated,
    /// using snake_case keys as required by the Bexio API.
    /// </summary>
    [Test]
    public void QueryParameterContact_SerializesLimitOffsetOrderByAndShowArchived()
    {
        var queryParameter = new QueryParameterContact(
            Limit: 100,
            Offset: 50,
            OrderBy: "name_1_desc",
            ShowArchived: true);

        Assert.That(queryParameter.QueryParameter, Is.Not.Null);
        Assert.That(queryParameter.QueryParameter!.Parameters, Contains.Key("limit"));
        Assert.That(queryParameter.QueryParameter.Parameters["limit"], Is.EqualTo(100));
        Assert.That(queryParameter.QueryParameter.Parameters, Contains.Key("offset"));
        Assert.That(queryParameter.QueryParameter.Parameters["offset"], Is.EqualTo(50));
        Assert.That(queryParameter.QueryParameter.Parameters, Contains.Key("order_by"));
        Assert.That(queryParameter.QueryParameter.Parameters["order_by"], Is.EqualTo("name_1_desc"));
        Assert.That(queryParameter.QueryParameter.Parameters, Contains.Key("show_archived"));
        Assert.That(queryParameter.QueryParameter.Parameters["show_archived"], Is.EqualTo("true"));
    }

    /// <summary>
    /// When all <see cref="QueryParameterContact"/> arguments are <see langword="null"/>,
    /// the inner <see cref="QueryParameter"/> is also <see langword="null"/> so the connection
    /// handler does not append any query string.
    /// </summary>
    [Test]
    public void QueryParameterContact_AllNullProducesNullQueryParameter()
    {
        var queryParameter = new QueryParameterContact();

        Assert.That(queryParameter.QueryParameter, Is.Null);
    }

    /// <summary>
    /// BulkCreate returns the connection handler's <see cref="ApiResult{T}"/> verbatim.
    /// </summary>
    [Test]
    public async Task BulkCreate_ReturnsApiResultFromConnectionHandler()
    {
        var payloads = new List<ContactCreate> { BuildCreatePayload() };
        var response = new ApiResult<List<Contact>> { IsSuccess = true, Data = [BuildContact(1)] };
        ConnectionHandler
            .PostBulkAsync<Contact, ContactCreate>(
                Arg.Any<List<ContactCreate>>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.BulkCreate(payloads);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Update returns the connection handler's <see cref="ApiResult{T}"/> verbatim.
    /// </summary>
    [Test]
    public async Task Update_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildUpdatePayload();
        var response = new ApiResult<Contact> { IsSuccess = true, Data = BuildContact(1) };
        ConnectionHandler
            .PostAsync<Contact, ContactUpdate>(
                Arg.Any<ContactUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Update(1, payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Restore returns the connection handler's <see cref="ApiResult{T}"/> verbatim.
    /// </summary>
    [Test]
    public async Task Restore_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .PatchAsync<object, object?>(Arg.Any<object?>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Restore(1);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete returns the connection handler's <see cref="ApiResult{T}"/> verbatim.
    /// </summary>
    [Test]
    public async Task Delete_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(1);

        Assert.That(result, Is.SameAs(response));
    }
}
