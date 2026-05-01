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
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.AdditionalAddresses;

/// <summary>
/// Live end-to-end tests for <see cref="AdditionalAddressService"/>. The tests are fully
/// self-contained: they create a parent contact, exercise the additional address CRUD
/// endpoints against it, and clean up both the additional address and the parent contact in
/// <c>try/finally</c> blocks. Tests are skipped automatically when <c>BexioApiNet__BaseUri</c>
/// or <c>BexioApiNet__JwtToken</c> are missing.
/// </summary>
[Category("E2E")]
public sealed class AdditionalAddressServiceE2eTests
{
    private const int CompanyContactType = 1;

    private BexioConnectionHandler? _connectionHandler;
    private AdditionalAddressService? _service;
    private ContactService? _contactService;

    /// <summary>
    /// Reads credentials from environment variables and constructs the services under test.
    /// Calls <see cref="Assert.Ignore(string)"/> when any value is missing.
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

        _service = new AdditionalAddressService(_connectionHandler);
        _contactService = new ContactService(_connectionHandler);
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
    /// Creates a parent contact, then lists, creates, reads, updates, searches, and deletes
    /// an additional address against it — exercising the full CRUD lifecycle for the
    /// nested resource. Both the additional address and the parent contact are cleaned up
    /// in a nested <c>try/finally</c>.
    /// </summary>
    [Test]
    public async Task FullCrudLifecycle_OnNestedAdditionalAddress()
    {
        var ownerId = await ResolveOwnerIdAsync();
        var contactName = $"E2E-AdditionalAddress-Parent-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var contactResult = await _contactService!.Create(new ContactCreate(
            ContactTypeId: CompanyContactType,
            Name1: contactName,
            UserId: ownerId,
            OwnerId: ownerId));
        Assert.That(contactResult.IsSuccess, Is.True, $"Parent contact create failed: {contactResult.ApiError?.Message}");
        var parentContactId = contactResult.Data!.Id;

        try
        {
            // List (initially empty for a fresh contact).
            var listEmpty = await _service!.Get(parentContactId);
            Assert.That(listEmpty.IsSuccess, Is.True);
            Assert.That(listEmpty.Data, Is.Not.Null);

            // Create.
            var create = await _service!.Create(parentContactId, new AdditionalAddressCreate(
                Name: "E2E-Address",
                NameAddition: null,
                StreetName: "Test Street",
                HouseNumber: "1",
                AddressAddition: null,
                Postcode: "8000",
                City: "Zurich",
                CountryId: 1,
                Subject: "E2E",
                Description: "Created by AdditionalAddressServiceE2eTests"));
            Assert.That(create.IsSuccess, Is.True, $"Address create failed: {create.ApiError?.Message}");
            Assert.That(create.Data, Is.Not.Null);
            var addressId = create.Data!.Id;

            try
            {
                // GetById.
                var read = await _service.GetById(parentContactId, addressId);
                Assert.That(read.IsSuccess, Is.True);
                Assert.That(read.Data?.Id, Is.EqualTo(addressId));
                Assert.That(read.Data?.Name, Is.EqualTo("E2E-Address"));

                // Update.
                var update = await _service.Update(parentContactId, addressId, new AdditionalAddressUpdate(
                    Name: "E2E-Address-Updated",
                    NameAddition: null,
                    StreetName: "Test Street",
                    HouseNumber: "2",
                    AddressAddition: null,
                    Postcode: "8000",
                    City: "Zurich",
                    CountryId: 1,
                    Subject: "E2E",
                    Description: "Updated by AdditionalAddressServiceE2eTests"));
                Assert.That(update.IsSuccess, Is.True);
                Assert.That(update.Data?.Name, Does.Contain("Updated"));

                // Search.
                var search = await _service.Search(
                    parentContactId,
                    new List<SearchCriteria>
                    {
                        new() { Field = "city", Value = "Zurich", Criteria = "=" }
                    });
                Assert.That(search.IsSuccess, Is.True);
                Assert.That(search.Data, Is.Not.Null);
            }
            finally
            {
                var delete = await _service.Delete(parentContactId, addressId);
                Assert.That(delete.IsSuccess, Is.True);
            }
        }
        finally
        {
            await _contactService.Delete(parentContactId);
        }
    }

    /// <summary>
    /// Probes the tenant for the first contact's owner id. Using an existing owner
    /// keeps the test safe across tenants without hardcoding ids.
    /// </summary>
    private async Task<int> ResolveOwnerIdAsync()
    {
        var existing = await _contactService!.Get(new QueryParameterContact(Limit: 1));
        return existing.IsSuccess && existing.Data is { Count: > 0 }
            ? existing.Data[0].OwnerId
            : 1;
    }
}
