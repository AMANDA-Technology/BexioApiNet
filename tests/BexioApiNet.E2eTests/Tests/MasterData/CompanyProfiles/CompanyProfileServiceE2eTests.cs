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
using BexioApiNet.Interfaces.Connectors.MasterData;
using BexioApiNet.Services.Connectors.MasterData;

namespace BexioApiNet.E2eTests.Tests.MasterData.CompanyProfiles;

/// <summary>
/// Live end-to-end tests for <see cref="CompanyProfileService"/>. Skipped automatically when
/// <c>BexioApiNet__BaseUri</c> / <c>BexioApiNet__JwtToken</c> are missing. The Bexio API
/// exposes <c>company_profile</c> as read-only on v2 — every account has exactly one profile —
/// so the lifecycle is List + GetById only.
/// </summary>
[Category("E2E")]
public sealed class CompanyProfileServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private ICompanyProfileService _sut = null!;

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

        _sut = new CompanyProfileService(_connectionHandler);
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
    /// <c>GET /2.0/company_profile</c> must return a non-empty list (always at least one profile)
    /// with the documented required <c>name</c> field populated, satisfying the OpenAPI
    /// <c>CompanyProfile</c> schema.
    /// </summary>
    [Test]
    public async Task Get_ReturnsCompanyProfileWithRequiredFields()
    {
        var res = await _sut.Get();

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data, Is.Not.Empty);

            var profile = res.Data![0];
            Assert.That(profile.Id, Is.GreaterThan(0));
            Assert.That(profile.Name, Is.Not.Null.And.Not.Empty);
        });
    }

    /// <summary>
    /// <c>GET /2.0/company_profile/{profile_id}</c> must return the profile with the given id,
    /// using the first id from the list endpoint.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsRequestedProfile()
    {
        var list = await _sut.Get();
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no company profile available on this tenant");
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
            Assert.That(res.Data.Name, Is.Not.Null.And.Not.Empty);
        });
    }
}
