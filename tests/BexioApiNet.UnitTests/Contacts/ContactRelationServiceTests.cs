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
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations;
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.UnitTests.Contacts;

/// <summary>
/// Offline unit tests for <see cref="ContactRelationService" />. Each test verifies that the service
/// forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected arguments and
/// returns the handler's result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class ContactRelationServiceTests : ServiceTestBase
{
    private const string ExpectedEndpoint = "2.0/contact_relation";

    private ContactRelationService _sut = null!;

    /// <summary>
    /// Creates a fresh <see cref="ContactRelationService" /> per test, bound to the
    /// <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new ContactRelationService(ConnectionHandler);
    }

    /// <summary>
    /// Get (no parameters) calls <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> once with
    /// the expected endpoint path and a null query parameter.
    /// </summary>
    [Test]
    public async Task Get_WithNoParams_CallsGetAsync()
    {
        var response = new ApiResult<List<ContactRelation>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ContactRelation>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get();

        await ConnectionHandler.Received(1).GetAsync<List<ContactRelation>?>(
            ExpectedEndpoint,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Get forwards the wrapped <see cref="QueryParameter"/> from a typed
    /// <see cref="QueryParameterContactRelation"/> to the connection handler.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsQueryParameter()
    {
        var queryParameter = new QueryParameterContactRelation(25, 50);
        var response = new ApiResult<List<ContactRelation>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ContactRelation>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Get(queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<ContactRelation>?>(
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
        var initialData = new List<ContactRelation> { BuildContactRelation(1), BuildContactRelation(2) };
        var initial = new ApiResult<List<ContactRelation>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<ContactRelation>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<ContactRelation>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<ContactRelation>(
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
        var response = new ApiResult<List<ContactRelation>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<ContactRelation>?>(
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get();

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// GetById forwards the call to <see cref="IBexioConnectionHandler.GetAsync{TResult}" /> with
    /// the endpoint path that includes the supplied id.
    /// </summary>
    [Test]
    public async Task GetById_PathContainsId()
    {
        const int id = 42;
        var response = new ApiResult<ContactRelation> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<ContactRelation>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.GetById(id);

        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// GetById returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    /// modification.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsApiResultFromConnectionHandler()
    {
        var response = new ApiResult<ContactRelation>
        {
            IsSuccess = true,
            Data = BuildContactRelation(1)
        };
        ConnectionHandler
            .GetAsync<ContactRelation>(
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
        var response = new ApiResult<ContactRelation> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<ContactRelation, ContactRelationCreate>(
                Arg.Any<ContactRelationCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Create(payload);

        await ConnectionHandler.Received(1).PostAsync<ContactRelation, ContactRelationCreate>(
            payload,
            ExpectedEndpoint,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Create returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    /// modification.
    /// </summary>
    [Test]
    public async Task Create_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<ContactRelation> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<ContactRelation, ContactRelationCreate>(
                Arg.Any<ContactRelationCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Search forwards the provided criteria list to
    /// <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" /> with the search endpoint path.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsync()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "contact_id", Value = "1", Criteria = "=" }
        };
        var response = new ApiResult<List<ContactRelation>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<ContactRelation>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria);

        await ConnectionHandler.Received(1).PostSearchAsync<ContactRelation>(
            criteria,
            $"{ExpectedEndpoint}/search",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Search forwards the optional <see cref="QueryParameterContactRelation"/> through to the
    /// connection handler as a <see cref="QueryParameter"/>.
    /// </summary>
    [Test]
    public async Task Search_WithQueryParameter_ForwardsQueryParameter()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "contact_id", Value = "1", Criteria = "=" }
        };
        var queryParameter = new QueryParameterContactRelation(10, 0);
        var response = new ApiResult<List<ContactRelation>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<ContactRelation>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Search(criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<ContactRelation>(
            criteria,
            $"{ExpectedEndpoint}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Update forwards the payload and the id-scoped endpoint path to
    /// <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />. Bexio v2.0 endpoints
    /// use HTTP POST on the resource URL to perform an edit.
    /// </summary>
    [Test]
    public async Task Update_CallsPostAsync_WithIdInPath()
    {
        const int id = 7;
        var payload = BuildUpdatePayload();
        var response = new ApiResult<ContactRelation> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<ContactRelation, ContactRelationUpdate>(
                Arg.Any<ContactRelationUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Update(id, payload);

        await ConnectionHandler.Received(1).PostAsync<ContactRelation, ContactRelationUpdate>(
            payload,
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        Assert.That(capturedPath, Is.EqualTo($"{ExpectedEndpoint}/{id}"));
    }

    /// <summary>
    /// Update returns the <see cref="ApiResult{T}" /> produced by the connection handler without
    /// modification.
    /// </summary>
    [Test]
    public async Task Update_ReturnsApiResultFromConnectionHandler()
    {
        var payload = BuildUpdatePayload();
        var response = new ApiResult<ContactRelation> { IsSuccess = true };
        ConnectionHandler
            .PostAsync<ContactRelation, ContactRelationUpdate>(
                Arg.Any<ContactRelationUpdate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Update(1, payload);

        Assert.That(result, Is.SameAs(response));
    }

    /// <summary>
    /// Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> exactly once.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        ConnectionHandler
            .Delete(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(response);

        await _sut.Delete(42);

        await ConnectionHandler.Received(1).Delete(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Delete builds the request path with the contact-relation id appended to the endpoint root.
    /// </summary>
    [Test]
    public async Task Delete_PathContainsId()
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

    private static ContactRelationCreate BuildCreatePayload()
        => new(ContactId: 1, ContactSubId: 2, Description: "managed by");

    private static ContactRelationUpdate BuildUpdatePayload()
        => new(ContactId: 1, ContactSubId: 2, Description: "updated");

    private static ContactRelation BuildContactRelation(int id)
        => new(id, ContactId: 1, ContactSubId: 2, Description: string.Empty, UpdatedAt: null);
}
