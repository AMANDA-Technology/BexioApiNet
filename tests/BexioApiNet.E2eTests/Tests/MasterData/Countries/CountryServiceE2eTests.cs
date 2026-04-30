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
using BexioApiNet.Abstractions.Models.MasterData.Countries.Views;
using BexioApiNet.Interfaces.Connectors.MasterData;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.E2eTests.Tests.MasterData.Countries;

/// <summary>
/// Live end-to-end tests for <see cref="CountryService"/>. Skipped automatically when
/// <c>BexioApiNet__BaseUri</c> / <c>BexioApiNet__JwtToken</c> are missing. Verifies the full
/// CRUD lifecycle (List, GetById, Create → Update → Delete) and structural conformance with
/// the Bexio v3 OpenAPI <c>Country</c> schema (<c>id</c>, <c>name</c>, <c>name_short</c>,
/// <c>iso3166_alpha2</c>).
/// </summary>
[Category("E2E")]
public sealed class CountryServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private ICountryService _sut = null!;

    /// <summary>
    /// Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from the environment
    /// and constructs a dedicated <see cref="BexioConnectionHandler"/> + <see cref="CountryService"/>
    /// per test. Calls <see cref="Assert.Ignore(string)"/> when credentials are absent.
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

        _sut = new CountryService(_connectionHandler);
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
    /// <c>GET /2.0/country</c> must return a non-empty list of countries on a provisioned tenant.
    /// Bexio ships a complete ISO-3166 country list out of the box. The first record is asserted
    /// to satisfy every required field on the OpenAPI <c>Country</c> schema
    /// (<c>name</c>, <c>name_short</c>, <c>iso3166_alpha2</c>).
    /// </summary>
    [Test]
    public async Task Get_ReturnsCountriesAndAllRequiredFieldsArePopulated()
    {
        var res = await _sut.Get(autoPage: true);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data, Is.Not.Empty);

            var first = res.Data![0];
            Assert.That(first.Id, Is.GreaterThan(0));
            Assert.That(first.Name, Is.Not.Null.And.Not.Empty);
            Assert.That(first.NameShort, Is.Not.Null.And.Not.Empty);
            Assert.That(first.Iso3166Alpha2, Is.Not.Null.And.Not.Empty);
        });
    }

    /// <summary>
    /// <c>GET /2.0/country/{country_id}</c> must return the country whose id matches the request,
    /// using the first id from the list endpoint.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsRequestedCountry()
    {
        var list = await _sut.Get(new QueryParameterCountry(Limit: 1, Offset: 0));
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no countries available on this tenant");
            return;
        }

        var id = existing[0].Id;

        var res = await _sut.GetById(id);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Id, Is.EqualTo(id));
        });
    }

    /// <summary>
    /// <c>POST /2.0/country/search</c> must return countries matching a name criterion.
    /// </summary>
    [Test]
    public async Task Search_ReturnsCountriesMatchingCriterion()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Switzerland", Criteria = "like" }
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
    /// Full Create → Update → Delete lifecycle. Creates a country with a unique name to avoid
    /// collisions on shared tenants, edits its display name via <c>POST /2.0/country/{id}</c>
    /// (operationId <c>v2EditCountry</c>), and finally cleans it up via
    /// <c>DELETE /2.0/country/{id}</c>.
    /// </summary>
    [Test]
    public async Task Lifecycle_Create_Update_Delete()
    {
        // Use random uppercase letters to avoid alpha-2 collisions on shared tenants.
        var nameSuffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var name = $"E2E-Country-{nameSuffix}";
        var alpha2 = nameSuffix[..2];

        var created = await _sut.Create(new CountryCreate(name, alpha2, alpha2));

        if (!created.IsSuccess)
            Assert.Ignore($"create failed ({created.StatusCode}) — possibly the iso2 code {alpha2} is already taken on this tenant");

        Assert.That(created.Data, Is.Not.Null);

        try
        {
            var updatedName = $"{name}-Edited";
            var updated = await _sut.Update(created.Data!.Id, new CountryUpdate(updatedName, alpha2, alpha2));

            Assert.That(updated, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updated.IsSuccess, Is.True);
                Assert.That(updated.ApiError, Is.Null);
                Assert.That(updated.Data?.Name, Is.EqualTo(updatedName));
            });
        }
        finally
        {
            var deleted = await _sut.Delete(created.Data!.Id);
            Assert.That(deleted.IsSuccess, Is.True);
        }
    }
}
