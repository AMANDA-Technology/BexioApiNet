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
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses;
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.UnitTests.Contacts;

/// <summary>
///     Offline unit tests for <see cref="AdditionalAddressService" />. Each test verifies that the
///     service forwards its calls to <see cref="IBexioConnectionHandler" /> with the expected
///     arguments (including the nested <c>contact_id</c> segment) and returns the handler's
///     result unchanged. No network, no filesystem access.
/// </summary>
[TestFixture]
public sealed class AdditionalAddressServiceTests : ServiceTestBase
{
    private const int ContactId = 42;
    private const int AdditionalAddressId = 7;
    private const string EndpointRoot = "2.0/contact/42/additional_address";

    private AdditionalAddressService _sut = null!;

    /// <summary>
    ///     Creates a fresh <see cref="AdditionalAddressService" /> per test, bound to the
    ///     <see cref="ServiceTestBase.ConnectionHandler" /> substitute provided by the base fixture.
    /// </summary>
    [SetUp]
    public void CreateSut()
    {
        _sut = new AdditionalAddressService(ConnectionHandler);
    }

    /// <summary>
    ///     Get forwards the expected nested path to <see cref="IBexioConnectionHandler.GetAsync{T}" />
    ///     and returns the handler's <see cref="ApiResult{T}" /> unchanged.
    /// </summary>
    [Test]
    public async Task Get_CallsGetAsyncWithExpectedPath()
    {
        var response = new ApiResult<List<AdditionalAddress>?> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .GetAsync<List<AdditionalAddress>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Get(ContactId);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).GetAsync<List<AdditionalAddress>?>(
            EndpointRoot,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get forwards the inner <see cref="QueryParameter" /> from the typed wrapper verbatim.
    /// </summary>
    [Test]
    public async Task Get_WithQueryParameter_ForwardsInnerQueryParameter()
    {
        var queryParameter = new QueryParameterAdditionalAddress(Limit: 25, Offset: 50);
        ConnectionHandler
            .GetAsync<List<AdditionalAddress>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<AdditionalAddress>?> { IsSuccess = true, Data = [] });

        await _sut.Get(ContactId, queryParameter);

        await ConnectionHandler.Received(1).GetAsync<List<AdditionalAddress>?>(
            EndpointRoot,
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Get with <c>autoPage=true</c> triggers <see cref="IBexioConnectionHandler.FetchAll{T}" />
    ///     when the response carries a non-null <c>X-Total-Count</c> header.
    /// </summary>
    [Test]
    public async Task Get_WithAutoPage_CallsFetchAll()
    {
        const int totalResults = 10;
        var initialData = new List<AdditionalAddress> { BuildAddress(1), BuildAddress(2) };
        var initial = new ApiResult<List<AdditionalAddress>?>
        {
            IsSuccess = true,
            Data = initialData,
            ResponseHeaders = new Dictionary<string, int?>
            {
                { ApiHeaderNames.TotalResults, totalResults }
            }
        };
        ConnectionHandler
            .GetAsync<List<AdditionalAddress>?>(Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns(initial);
        ConnectionHandler
            .FetchAll<AdditionalAddress>(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<QueryParameter?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        await _sut.Get(ContactId, autoPage: true);

        await ConnectionHandler.Received(1).FetchAll<AdditionalAddress>(
            initialData.Count,
            totalResults,
            EndpointRoot,
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     GetById hits the single-resource path with both the contact id and the address id.
    /// </summary>
    [Test]
    public async Task GetById_CallsGetAsyncWithExpectedPath()
    {
        var response = new ApiResult<AdditionalAddress> { IsSuccess = true, Data = BuildAddress(AdditionalAddressId) };
        string? capturedPath = null;
        ConnectionHandler
            .GetAsync<AdditionalAddress>(
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.GetById(ContactId, AdditionalAddressId);

        Assert.That(result, Is.SameAs(response));
        Assert.That(capturedPath, Is.EqualTo($"{EndpointRoot}/{AdditionalAddressId}"));
    }

    /// <summary>
    ///     Create forwards the payload to <see cref="IBexioConnectionHandler.PostAsync{TResult,TCreate}" />
    ///     against the collection path.
    /// </summary>
    [Test]
    public async Task Create_CallsPostAsyncWithExpectedPathAndPayload()
    {
        var payload = BuildCreatePayload();
        var response = new ApiResult<AdditionalAddress> { IsSuccess = true, Data = BuildAddress(1) };
        ConnectionHandler
            .PostAsync<AdditionalAddress, AdditionalAddressCreate>(
                Arg.Any<AdditionalAddressCreate>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Create(ContactId, payload);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).PostAsync<AdditionalAddress, AdditionalAddressCreate>(
            payload,
            EndpointRoot,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search hits the nested <c>/search</c> path via
    ///     <see cref="IBexioConnectionHandler.PostSearchAsync{TResult}" /> with the provided criteria.
    /// </summary>
    [Test]
    public async Task Search_CallsPostSearchAsyncWithExpectedPathAndCriteria()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Acme", Criteria = "like" }
        };
        var response = new ApiResult<List<AdditionalAddress>> { IsSuccess = true, Data = [] };
        ConnectionHandler
            .PostSearchAsync<AdditionalAddress>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Search(ContactId, criteria);

        Assert.That(result, Is.SameAs(response));
        await ConnectionHandler.Received(1).PostSearchAsync<AdditionalAddress>(
            criteria,
            $"{EndpointRoot}/search",
            null,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Search forwards the optional typed query parameter to the handler (for limit/offset).
    /// </summary>
    [Test]
    public async Task Search_WithQueryParameter_ForwardsInnerQueryParameter()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "city", Value = "Zurich", Criteria = "=" }
        };
        var queryParameter = new QueryParameterAdditionalAddress(Limit: 100, Offset: 0);
        ConnectionHandler
            .PostSearchAsync<AdditionalAddress>(
                Arg.Any<List<SearchCriteria>>(),
                Arg.Any<string>(),
                Arg.Any<QueryParameter?>(),
                Arg.Any<CancellationToken>())
            .Returns(new ApiResult<List<AdditionalAddress>> { IsSuccess = true, Data = [] });

        await _sut.Search(ContactId, criteria, queryParameter);

        await ConnectionHandler.Received(1).PostSearchAsync<AdditionalAddress>(
            criteria,
            $"{EndpointRoot}/search",
            queryParameter.QueryParameter,
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Update forwards the payload to the single-resource path via the connection handler's
    ///     update hook. The Bexio edit endpoint uses HTTP <c>POST</c> rather than <c>PUT</c>.
    /// </summary>
    [Test]
    public async Task Update_CallsUpdateWithExpectedPathAndPayload()
    {
        var payload = BuildUpdatePayload();
        var response = new ApiResult<AdditionalAddress> { IsSuccess = true, Data = BuildAddress(AdditionalAddressId) };
        string? capturedPath = null;
        ConnectionHandler
            .PostAsync<AdditionalAddress, AdditionalAddressUpdate>(
                Arg.Any<AdditionalAddressUpdate>(),
                Arg.Do<string>(path => capturedPath = path),
                Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Update(ContactId, AdditionalAddressId, payload);

        Assert.That(result, Is.SameAs(response));
        Assert.That(capturedPath, Is.EqualTo($"{EndpointRoot}/{AdditionalAddressId}"));
        await ConnectionHandler.Received(1).PostAsync<AdditionalAddress, AdditionalAddressUpdate>(
            payload,
            $"{EndpointRoot}/{AdditionalAddressId}",
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    ///     Delete forwards the call to <see cref="IBexioConnectionHandler.Delete" /> with both
    ///     the parent contact id and the address id in the path.
    /// </summary>
    [Test]
    public async Task Delete_CallsConnectionHandlerDelete()
    {
        var response = new ApiResult<object> { IsSuccess = true };
        string? capturedPath = null;
        ConnectionHandler
            .Delete(Arg.Do<string>(path => capturedPath = path), Arg.Any<CancellationToken>())
            .Returns(response);

        var result = await _sut.Delete(ContactId, AdditionalAddressId);

        Assert.That(result, Is.SameAs(response));
        Assert.That(capturedPath, Is.EqualTo($"{EndpointRoot}/{AdditionalAddressId}"));
    }

    private static AdditionalAddressCreate BuildCreatePayload() => new(
        Name: "Head Office",
        NameAddition: "Reception",
        StreetName: "Walter Street",
        HouseNumber: "22",
        AddressAddition: "Building C",
        Postcode: "9000",
        City: "St. Gallen",
        CountryId: 1,
        Subject: "Additional address",
        Description: "Internal description");

    private static AdditionalAddressUpdate BuildUpdatePayload() => new(
        Name: "Head Office",
        NameAddition: "Reception",
        StreetName: "Walter Street",
        HouseNumber: "22",
        AddressAddition: "Building C",
        Postcode: "9000",
        City: "St. Gallen",
        CountryId: 1,
        Subject: "Additional address",
        Description: "Internal description");

    private static AdditionalAddress BuildAddress(int id) => new(
        Id: id,
        Name: "Head Office",
        NameAddition: null,
        Address: "Walter Street 22",
        StreetName: "Walter Street",
        HouseNumber: "22",
        AddressAddition: null,
        Postcode: "9000",
        City: "St. Gallen",
        CountryId: 1,
        Subject: "Additional address",
        Description: null);
}
