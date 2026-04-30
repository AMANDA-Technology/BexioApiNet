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
using BexioApiNet.Abstractions.Models.Contacts.ContactRelations.Views;
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.ContactRelations;

/// <summary>
/// Live end-to-end tests for <see cref="ContactRelationService"/>. The tests are fully
/// self-contained: they create two parent contacts, exercise the contact relation CRUD
/// endpoints between them, and clean up everything in <c>try/finally</c> blocks. Tests
/// are skipped automatically when <c>BexioApiNet__BaseUri</c> or <c>BexioApiNet__JwtToken</c>
/// are missing.
/// </summary>
[Category("E2E")]
public sealed class ContactRelationServiceE2eTests
{
    private const int CompanyContactType = 1;

    private BexioConnectionHandler? _connectionHandler;
    private IContactRelationService? _service;
    private ContactService? _contactService;

    /// <summary>
    /// Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from the environment.
    /// When either is missing the test is skipped so CI and agent runs without live credentials
    /// do not fail.
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

        _service = new ContactRelationService(_connectionHandler);
        _contactService = new ContactService(_connectionHandler);
    }

    /// <summary>
    /// Disposes the locally owned connection handler.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    /// Lists contact relations to verify the GET /2.0/contact_relation endpoint round-trips.
    /// </summary>
    [Test]
    public async Task GetList_ReturnsContactRelations()
    {
        var result = await _service!.Get(new QueryParameterContactRelation(Limit: 5, Offset: 0));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }

    /// <summary>
    /// Searches contact relations with a contact_id filter.
    /// </summary>
    [Test]
    public async Task Search_ReturnsContactRelations()
    {
        var ownerId = await ResolveOwnerIdAsync();
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "contact_id", Value = ownerId.ToString(), Criteria = "=" }
        };

        var result = await _service!.Search(criteria);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }

    /// <summary>
    /// Creates two parent contacts, then exercises Create → Read → Update → Delete on a
    /// contact relation between them. Cleans up the relation and both parent contacts in
    /// nested <c>try/finally</c> blocks.
    /// </summary>
    [Test]
    public async Task FullCrudLifecycle_OnContactRelation()
    {
        var ownerId = await ResolveOwnerIdAsync();
        var contactA = await CreateContactAsync(ownerId, $"E2E-Relation-A-{DateTime.UtcNow:yyyyMMddHHmmssfff}");
        try
        {
            var contactB = await CreateContactAsync(ownerId, $"E2E-Relation-B-{DateTime.UtcNow:yyyyMMddHHmmssfff}");
            try
            {
                var create = await _service!.Create(new ContactRelationCreate(
                    ContactId: contactA,
                    ContactSubId: contactB,
                    Description: "E2E-ContactRelation"));
                Assert.That(create.IsSuccess, Is.True, $"Create failed: {create.ApiError?.Message}");
                Assert.That(create.Data, Is.Not.Null);
                var relationId = create.Data!.Id;

                try
                {
                    var read = await _service.GetById(relationId);
                    Assert.That(read.IsSuccess, Is.True);
                    Assert.That(read.Data?.Id, Is.EqualTo(relationId));

                    var update = await _service.Update(relationId, new ContactRelationUpdate(
                        ContactId: contactA,
                        ContactSubId: contactB,
                        Description: "E2E-ContactRelation-Updated"));
                    Assert.That(update.IsSuccess, Is.True);
                    Assert.That(update.Data?.Description, Is.EqualTo("E2E-ContactRelation-Updated"));
                }
                finally
                {
                    await _service.Delete(relationId);
                }
            }
            finally
            {
                await _contactService!.Delete(contactB);
            }
        }
        finally
        {
            await _contactService!.Delete(contactA);
        }
    }

    private async Task<int> CreateContactAsync(int ownerId, string name)
    {
        var result = await _contactService!.Create(new ContactCreate(
            ContactTypeId: CompanyContactType,
            Name1: name,
            UserId: ownerId,
            OwnerId: ownerId));
        Assert.That(result.IsSuccess, Is.True, $"Parent contact create failed: {result.ApiError?.Message}");
        return result.Data!.Id;
    }

    /// <summary>
    /// Probes the tenant for the first contact's owner id. Using an existing owner keeps
    /// the test safe across tenants without hardcoding ids.
    /// </summary>
    private async Task<int> ResolveOwnerIdAsync()
    {
        var existing = await _contactService!.Get(new QueryParameterContact(Limit: 1));
        return existing.IsSuccess && existing.Data is { Count: > 0 }
            ? existing.Data[0].OwnerId
            : 1;
    }
}
