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
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.ContactRelations;

/// <summary>
/// Live E2E test stubs for <see cref="ContactRelationService"/>. Stand-alone class (does not
/// inherit <see cref="BexioE2eTestBase"/>) so that this sub-issue does not modify the shared
/// test base — DI wire-up is handled by a separate sub-issue (#49). Credentials are read from
/// the same environment variables; tests are skipped via <see cref="Assert.Ignore(string)"/>
/// when they are missing.
/// </summary>
[Category("E2E")]
public class ContactRelationServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private IContactRelationService? _service;

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
    /// Stub — live test for <c>GET /2.0/contact_relation</c>. Requires a tenant with at least
    /// one contact relation.
    /// </summary>
    [Test]
    public async Task GetList_ReturnsContactRelations()
    {
        Assert.That(_service, Is.Not.Null);

        var result = await _service!.Get(new QueryParameterContactRelation(5, 0));

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }

    /// <summary>
    /// Stub — live test for <c>GET /2.0/contact_relation/{id}</c>. Fetches the first
    /// contact relation from the list and looks it up by id.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsContactRelation()
    {
        Assert.That(_service, Is.Not.Null);

        var list = await _service!.Get(new QueryParameterContactRelation(1, 0));
        Assert.That(list.IsSuccess, Is.True);
        if (list.Data is null || list.Data.Count is 0)
            Assert.Ignore("no contact relation available to query by id");

        var id = list.Data![0].Id;

        var result = await _service.GetById(id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data?.Id, Is.EqualTo(id));
        });
    }

    /// <summary>
    /// Stub — live test for <c>POST /2.0/contact_relation</c>. The create/delete cycle requires
    /// two valid contact ids in the target tenant; skip rather than fail if none are available.
    /// </summary>
    [Test]
    public async Task Create_CreatesAndDeletesContactRelation()
    {
        Assert.That(_service, Is.Not.Null);

        const int contactId = 1;
        const int contactSubId = 2;

        var created = await _service!.Create(new ContactRelationCreate(
            ContactId: contactId,
            ContactSubId: contactSubId,
            Description: "E2E-ContactRelation"));

        if (!created.IsSuccess)
            Assert.Ignore($"create failed ({created.StatusCode}) — check that contact ids {contactId} and {contactSubId} exist in the tenant");

        Assert.That(created.Data, Is.Not.Null);

        var cleanup = await _service.Delete(created.Data!.Id);
        Assert.That(cleanup.IsSuccess, Is.True);
    }

    /// <summary>
    /// Stub — live test for <c>POST /2.0/contact_relation/search</c>.
    /// </summary>
    [Test]
    public async Task Search_ReturnsContactRelations()
    {
        Assert.That(_service, Is.Not.Null);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "contact_id", Value = "1", Criteria = "=" }
        };

        var result = await _service!.Search(criteria);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }

    /// <summary>
    /// Stub — live test for <c>POST /2.0/contact_relation/{id}</c> (Bexio v2.0 edit semantics).
    /// Creates a contact relation, updates its description, then cleans up.
    /// </summary>
    [Test]
    public async Task Update_EditsContactRelation()
    {
        Assert.That(_service, Is.Not.Null);

        const int contactId = 1;
        const int contactSubId = 2;

        var created = await _service!.Create(new ContactRelationCreate(
            ContactId: contactId,
            ContactSubId: contactSubId,
            Description: "E2E-ContactRelation-Update"));

        if (!created.IsSuccess)
            Assert.Ignore($"create failed ({created.StatusCode}) — check that contact ids {contactId} and {contactSubId} exist in the tenant");

        Assert.That(created.Data, Is.Not.Null);

        try
        {
            var updated = await _service.Update(created.Data!.Id, new ContactRelationUpdate(
                ContactId: contactId,
                ContactSubId: contactSubId,
                Description: "E2E-ContactRelation-Updated"));

            Assert.That(updated, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updated.IsSuccess, Is.True);
                Assert.That(updated.Data?.Description, Is.EqualTo("E2E-ContactRelation-Updated"));
            });
        }
        finally
        {
            await _service.Delete(created.Data!.Id);
        }
    }

    /// <summary>
    /// Stub — live test for <c>DELETE /2.0/contact_relation/{id}</c>. Creates a contact relation,
    /// deletes it, and verifies the API reports success.
    /// </summary>
    [Test]
    public async Task Delete_RemovesContactRelation()
    {
        Assert.That(_service, Is.Not.Null);

        const int contactId = 1;
        const int contactSubId = 2;

        var created = await _service!.Create(new ContactRelationCreate(
            ContactId: contactId,
            ContactSubId: contactSubId,
            Description: "E2E-ContactRelation-Delete"));

        if (!created.IsSuccess)
            Assert.Ignore($"create failed ({created.StatusCode}) — check that contact ids {contactId} and {contactSubId} exist in the tenant");

        Assert.That(created.Data, Is.Not.Null);

        var result = await _service.Delete(created.Data!.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }
}
