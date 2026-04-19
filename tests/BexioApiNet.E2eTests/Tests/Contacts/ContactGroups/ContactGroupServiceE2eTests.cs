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
using BexioApiNet.Abstractions.Models.Contacts.ContactGroups;
using BexioApiNet.Abstractions.Models.Contacts.ContactGroups.Views;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.ContactGroups;

/// <summary>
/// Live end-to-end tests for <see cref="ContactGroupService"/>. Tests are skipped when
/// the required environment variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>)
/// are not present. The service is constructed directly because it is not yet wired into
/// <c>IBexioApiClient</c> (tracked separately in issue #49).
/// </summary>
[Category("E2E")]
public class ContactGroupServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private IContactGroupService _sut = null!;

    /// <summary>
    /// Reads the required credentials from environment variables. Calls
    /// <see cref="Assert.Ignore(string)"/> when either is missing so the suite does not
    /// fail CI or AI agent runs that lack live credentials.
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

        _sut = new ContactGroupService(_connectionHandler);
    }

    /// <summary>
    /// Disposes the connection handler if it was created for the test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
        _connectionHandler = null;
    }

    /// <summary>
    /// Verifies that listing contact groups returns a successful response.
    /// </summary>
    [Test]
    public async Task Get()
    {
        var res = await _sut.Get(autoPage: true);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies that fetching a single contact group by id succeeds when at least one group exists.
    /// </summary>
    [Test]
    public async Task GetById()
    {
        var list = await _sut.Get();
        Assert.That(list, Is.Not.Null);
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no contact groups available on this tenant");
            return;
        }

        var res = await _sut.GetById(existing[0].Id);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data?.Id, Is.EqualTo(existing[0].Id));
        });
    }

    /// <summary>
    /// Verifies that creating a contact group succeeds and returns the persisted record.
    /// Cleans up the created group as part of the test.
    /// </summary>
    [Test]
    public async Task Create()
    {
        var name = $"Test group {Guid.NewGuid():N}";
        var res = await _sut.Create(new ContactGroupCreate(name));

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data?.Name, Is.EqualTo(name));
        });

        if (res.Data is { } created)
            await _sut.Delete(created.Id);
    }

    /// <summary>
    /// Verifies that searching contact groups by name returns a successful response.
    /// </summary>
    [Test]
    public async Task Search()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Test", Criteria = "like" }
        };

        var res = await _sut.Search(criteria);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Verifies that updating a contact group changes the name and returns the updated record.
    /// Creates and deletes a temporary group as part of the test.
    /// </summary>
    [Test]
    public async Task Update()
    {
        var created = await _sut.Create(new ContactGroupCreate($"Test group {Guid.NewGuid():N}"));
        Assert.That(created.IsSuccess, Is.True);
        Assert.That(created.Data, Is.Not.Null);

        try
        {
            var newName = $"Updated group {Guid.NewGuid():N}";
            var res = await _sut.Update(created.Data!.Id, new ContactGroupUpdate(newName));

            Assert.That(res, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(res.IsSuccess, Is.True);
                Assert.That(res.ApiError, Is.Null);
                Assert.That(res.Data?.Name, Is.EqualTo(newName));
            });
        }
        finally
        {
            await _sut.Delete(created.Data!.Id);
        }
    }

    /// <summary>
    /// Verifies that deleting a contact group succeeds.
    /// </summary>
    [Test]
    public async Task Delete()
    {
        var created = await _sut.Create(new ContactGroupCreate($"Test group {Guid.NewGuid():N}"));
        Assert.That(created.IsSuccess, Is.True);
        Assert.That(created.Data, Is.Not.Null);

        var res = await _sut.Delete(created.Data!.Id);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
        });
    }
}
