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
using BexioApiNet.Abstractions.Models.Contacts.AdditionalAddresses.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.AdditionalAddresses;

/// <summary>
/// Live end-to-end tests for <see cref="AdditionalAddressService"/>. Because the
/// <c>BexioApiClient</c> aggregator does not yet expose the additional-address
/// service (wired in a follow-up issue), these tests build the service directly
/// from a <see cref="BexioConnectionHandler"/>. Tests are skipped automatically
/// when the required environment variables (<c>BexioApiNet__BaseUri</c>,
/// <c>BexioApiNet__JwtToken</c>, <c>BexioApiNet__ContactId</c>) are not configured.
/// </summary>
[Category("E2E")]
public sealed class AdditionalAddressServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private AdditionalAddressService? _service;
    private int _contactId;

    /// <summary>
    /// Reads credentials and a parent contact identifier from environment variables
    /// and constructs the service under test. Calls <see cref="Assert.Ignore(string)"/>
    /// when any value is missing so the suite stays green in environments without live
    /// credentials.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");
        var contactIdRaw = Environment.GetEnvironmentVariable("BexioApiNet__ContactId");

        if (string.IsNullOrWhiteSpace(baseUri) || string.IsNullOrWhiteSpace(jwtToken) || !int.TryParse(contactIdRaw, out _contactId))
        {
            Assert.Ignore("credentials or parent contact id not configured");
            return;
        }

        _connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        _service = new AdditionalAddressService(_connectionHandler);
    }

    /// <summary>
    /// Disposes the connection handler owned by the test when present.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    /// Lists additional addresses attached to the configured parent contact.
    /// </summary>
    [Test]
    public async Task Get_ListsAdditionalAddresses()
    {
        var res = await _service!.Get(_contactId, new QueryParameterAdditionalAddress(Limit: 5, Offset: 0));

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Fetches the first listed additional address by identifier.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsAdditionalAddress()
    {
        var list = await _service!.Get(_contactId, new QueryParameterAdditionalAddress(Limit: 1, Offset: 0));
        Assert.That(list.IsSuccess, Is.True);
        Assert.That(list.Data, Is.Not.Null.And.Not.Empty);

        var id = list.Data![0].Id;

        var res = await _service!.GetById(_contactId, id);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.Id, Is.EqualTo(id));
        });
    }

    /// <summary>
    /// Creates, updates, and deletes an additional address to exercise the full
    /// write path in a single end-to-end scenario.
    /// </summary>
    [Test]
    public async Task CreateUpdateDelete_RoundTripsAdditionalAddress()
    {
        var create = await _service!.Create(_contactId, new AdditionalAddressCreate(
            Name: "E2E Additional Address",
            NameAddition: null,
            StreetName: "Test Street",
            HouseNumber: "1",
            AddressAddition: null,
            Postcode: "8000",
            City: "Zurich",
            CountryId: 1,
            Subject: "E2E",
            Description: "Created by AdditionalAddressServiceE2eTests"));

        Assert.That(create, Is.Not.Null);
        Assert.That(create.IsSuccess, Is.True);
        Assert.That(create.Data, Is.Not.Null);

        var createdId = create.Data!.Id;

        try
        {
            var update = await _service!.Update(_contactId, createdId, new AdditionalAddressUpdate(
                Name: "E2E Additional Address (updated)",
                NameAddition: null,
                StreetName: "Test Street",
                HouseNumber: "2",
                AddressAddition: null,
                Postcode: "8000",
                City: "Zurich",
                CountryId: 1,
                Subject: "E2E",
                Description: "Updated by AdditionalAddressServiceE2eTests"));

            Assert.That(update, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(update.IsSuccess, Is.True);
                Assert.That(update.Data?.Id, Is.EqualTo(createdId));
                Assert.That(update.Data?.Name, Does.Contain("updated"));
            });
        }
        finally
        {
            var delete = await _service!.Delete(_contactId, createdId);
            Assert.That(delete.IsSuccess, Is.True);
        }
    }

    /// <summary>
    /// Searches additional addresses with a single criterion to confirm wire
    /// compatibility with Bexio's search endpoint.
    /// </summary>
    [Test]
    public async Task Search_ReturnsMatchingAdditionalAddresses()
    {
        var res = await _service!.Search(
            _contactId,
            new List<SearchCriteria>
            {
                new() { Field = "city", Value = "Zurich", Criteria = "=" }
            },
            new QueryParameterAdditionalAddress(Limit: 5, Offset: 0));

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }
}
