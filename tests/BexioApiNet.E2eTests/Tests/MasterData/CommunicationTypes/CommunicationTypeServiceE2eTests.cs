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
using BexioApiNet.Interfaces.Connectors.MasterData;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.E2eTests.Tests.MasterData.CommunicationTypes;

/// <summary>
/// Live end-to-end tests for <see cref="CommunicationTypeService"/>. Skipped automatically when
/// <c>BexioApiNet__BaseUri</c> / <c>BexioApiNet__JwtToken</c> are missing. Bexio exposes the
/// resource under the legacy URL segment <c>communication_kind</c> with read + search semantics.
/// Verifies structural conformance with the OpenAPI <c>CommunicationType</c> schema
/// (<c>id</c>, <c>name</c>).
/// </summary>
[Category("E2E")]
public sealed class CommunicationTypeServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private ICommunicationTypeService _sut = null!;

    /// <summary>
    /// Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from the environment.
    /// Calls <see cref="Assert.Ignore(string)"/> when credentials are absent.
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

        _sut = new CommunicationTypeService(_connectionHandler);
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
    /// <c>GET /2.0/communication_kind</c> must return a non-empty list of communication types.
    /// Bexio ships defaults (<c>Phone</c>, <c>Mobile</c>, <c>E-Mail</c>) on every tenant. Each
    /// entry must satisfy the required field on the OpenAPI schema (<c>name</c>).
    /// </summary>
    [Test]
    public async Task Get_ReturnsCommunicationTypesAndAllRequiredFieldsArePopulated()
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
        });
    }

    /// <summary>
    /// <c>POST /2.0/communication_kind/search</c> with a <c>name like '%Phone%'</c> criterion
    /// must return a successful response. The exact result set depends on the tenant's defaults.
    /// </summary>
    [Test]
    public async Task Search_ReturnsCommunicationTypesMatchingCriterion()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "Phone", Criteria = "like" }
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
}
