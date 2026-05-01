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
using BexioApiNet.Abstractions.Models.Contacts.Contacts.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.Contacts;

/// <summary>
/// Live end-to-end tests for <see cref="ContactService"/>. Exercises the full
/// Create → Read → Update → Delete lifecycle against the live Bexio API. All test
/// records are prefixed with <c>"E2E-"</c> for easy identification in the tenant
/// and cleaned up via <c>try/finally</c>. Tests are skipped automatically when
/// <c>BexioApiNet__BaseUri</c> or <c>BexioApiNet__JwtToken</c> are missing.
/// </summary>
[Category("E2E")]
public sealed class ContactServiceE2eTests
{
    private const int CompanyContactType = 1;

    private BexioConnectionHandler? _connectionHandler;
    private ContactService? _service;

    /// <summary>
    /// Reads the required credentials from environment variables. Calls
    /// <see cref="Assert.Ignore(string)"/> when either is missing.
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

        _service = new ContactService(_connectionHandler);
    }

    /// <summary>
    /// Disposes the connection handler created for the test run.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    /// Lists the first page of contacts and confirms the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task List_ReturnsContacts()
    {
        var result = await _service!.Get(new QueryParameterContact(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Searches contacts with an empty criteria set — Bexio treats this as match-all.
    /// </summary>
    [Test]
    public async Task Search_WithEmptyCriteria_ReturnsContacts()
    {
        var result = await _service!.Search(new List<SearchCriteria>());

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }

    /// <summary>
    /// Creates a contact, reads it back by id, updates the name, and deletes it. Cleans
    /// up via <c>try/finally</c> to ensure no orphan records are left in the test tenant
    /// even if assertions fail.
    /// </summary>
    [Test]
    public async Task CreateReadUpdateDelete_RoundTripsContact()
    {
        var ownerId = await ResolveOwnerIdAsync();
        var name = $"E2E-Contact-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var create = await _service!.Create(new ContactCreate(
            ContactTypeId: CompanyContactType,
            Name1: name,
            UserId: ownerId,
            OwnerId: ownerId));

        Assert.That(create, Is.Not.Null);
        Assert.That(create.IsSuccess, Is.True, $"Create failed: {create.ApiError?.Message}");
        Assert.That(create.Data, Is.Not.Null);

        var createdId = create.Data!.Id;

        try
        {
            var read = await _service.GetById(createdId);
            Assert.That(read.IsSuccess, Is.True);
            Assert.That(read.Data?.Id, Is.EqualTo(createdId));
            Assert.That(read.Data?.Name1, Is.EqualTo(name));

            var updatedName = $"{name}-updated";
            var update = await _service.Update(createdId, new ContactUpdate(
                ContactTypeId: CompanyContactType,
                Name1: updatedName,
                UserId: ownerId,
                OwnerId: ownerId));
            Assert.That(update.IsSuccess, Is.True);
            Assert.That(update.Data?.Name1, Is.EqualTo(updatedName));
        }
        finally
        {
            var delete = await _service.Delete(createdId);
            Assert.That(delete.IsSuccess, Is.True);
        }
    }

    /// <summary>
    /// Creates a contact, archives it via Delete, restores it via PATCH, then permanently
    /// removes it. Verifies the spec-defined PATCH /restore endpoint is wired correctly.
    /// </summary>
    [Test]
    public async Task DeleteAndRestore_BringsContactBackToLife()
    {
        var ownerId = await ResolveOwnerIdAsync();
        var name = $"E2E-Contact-Restore-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var create = await _service!.Create(new ContactCreate(
            ContactTypeId: CompanyContactType,
            Name1: name,
            UserId: ownerId,
            OwnerId: ownerId));
        Assert.That(create.IsSuccess, Is.True, $"Create failed: {create.ApiError?.Message}");
        var createdId = create.Data!.Id;

        try
        {
            var archive = await _service.Delete(createdId);
            Assert.That(archive.IsSuccess, Is.True);

            var restore = await _service.Restore(createdId);
            Assert.That(restore.IsSuccess, Is.True);
        }
        finally
        {
            // Final deletion (after restore) — ignore failures because the contact may
            // already be gone if the test was interrupted between archive and restore.
            await _service.Delete(createdId);
        }
    }

    /// <summary>
    /// Probes the tenant for the first contact's owner id. Using an existing owner
    /// keeps the test safe across tenants without hardcoding ids. Falls back to
    /// <c>1</c> only when the tenant is empty (which is unlikely for an E2E run).
    /// </summary>
    private async Task<int> ResolveOwnerIdAsync()
    {
        var existing = await _service!.Get(new QueryParameterContact(Limit: 1));
        return existing.IsSuccess && existing.Data is { Count: > 0 }
            ? existing.Data[0].OwnerId
            : 1;
    }
}
