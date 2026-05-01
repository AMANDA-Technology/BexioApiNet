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
using BexioApiNet.Abstractions.Models.Contacts.ContactGroups.Views;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Contacts;

namespace BexioApiNet.E2eTests.Tests.Contacts.ContactGroups;

/// <summary>
/// Live end-to-end tests for <see cref="ContactGroupService"/>. Tests are skipped when
/// the required environment variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>)
/// are not present.
/// </summary>
[Category("E2E")]
public sealed class ContactGroupServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private IContactGroupService _sut = null!;

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
    /// Lists contact groups with auto-paging enabled.
    /// </summary>
    [Test]
    public async Task Get_ListsContactGroups()
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
    /// Searches contact groups with a name prefix filter.
    /// </summary>
    [Test]
    public async Task Search_ReturnsContactGroups()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "E2E", Criteria = "like" }
        };

        var res = await _sut.Search(criteria);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.IsSuccess, Is.True);
    }

    /// <summary>
    /// Exercises the full Create → Read → Update → Delete lifecycle for a contact group.
    /// All test data is prefixed with <c>"E2E-"</c> for easy identification and cleaned up
    /// in <c>try/finally</c> to avoid leaking records if assertions fail.
    /// </summary>
    [Test]
    public async Task FullCrudLifecycle_OnContactGroup()
    {
        var name = $"E2E-Group-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

        var create = await _sut.Create(new ContactGroupCreate(name));
        Assert.That(create.IsSuccess, Is.True, $"Create failed: {create.ApiError?.Message}");
        Assert.That(create.Data, Is.Not.Null);
        Assert.That(create.Data!.Name, Is.EqualTo(name));

        var createdId = create.Data.Id;

        try
        {
            var read = await _sut.GetById(createdId);
            Assert.That(read.IsSuccess, Is.True);
            Assert.That(read.Data?.Id, Is.EqualTo(createdId));
            Assert.That(read.Data?.Name, Is.EqualTo(name));

            var updatedName = $"{name}-updated";
            var update = await _sut.Update(createdId, new ContactGroupUpdate(updatedName));
            Assert.That(update.IsSuccess, Is.True);
            Assert.That(update.Data?.Name, Is.EqualTo(updatedName));
        }
        finally
        {
            var delete = await _sut.Delete(createdId);
            Assert.That(delete.IsSuccess, Is.True);
        }
    }
}
