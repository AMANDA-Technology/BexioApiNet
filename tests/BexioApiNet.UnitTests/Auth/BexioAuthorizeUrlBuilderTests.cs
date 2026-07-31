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

using System.Web;

namespace BexioApiNet.UnitTests.Auth;

/// <summary>
/// Unit tests for <see cref="BexioAuthorizeUrlBuilder" /> and the endpoint derivation in
/// <see cref="BexioOAuthOptions" />.
/// </summary>
[TestFixture]
[Category("Unit")]
public class BexioAuthorizeUrlBuilderTests
{
    private static readonly BexioOAuthOptions Options = new()
    {
        ClientId = "client-id",
        Authority = "https://auth.example.local/realms/bexio",
        RedirectUri = "https://app.example.local/callback",
        Scopes = ["accounting", BexioAuthDefaults.OfflineAccessScope]
    };

    /// <summary>
    /// The consent URL points at the realm's authorization endpoint and carries the parameters
    /// bexio needs to issue an authorization code.
    /// </summary>
    [Test]
    public void Build_ProducesAuthorizationEndpointWithRequiredParameters()
    {
        var url = BexioAuthorizeUrlBuilder.Build(Options, "state-1");
        var query = HttpUtility.ParseQueryString(url.Query);

        Assert.Multiple(() =>
        {
            Assert.That(url.GetLeftPart(UriPartial.Path),
                Is.EqualTo("https://auth.example.local/realms/bexio/protocol/openid-connect/auth"));
            Assert.That(query["response_type"], Is.EqualTo("code"));
            Assert.That(query["client_id"], Is.EqualTo("client-id"));
            Assert.That(query["redirect_uri"], Is.EqualTo("https://app.example.local/callback"));
            Assert.That(query["state"], Is.EqualTo("state-1"));
            Assert.That(query["scope"], Is.EqualTo($"accounting {BexioAuthDefaults.OfflineAccessScope}"));
            Assert.That(query["code_challenge"], Is.Null);
        });
    }

    /// <summary>
    /// PKCE parameters are added only when a challenge is supplied.
    /// </summary>
    [Test]
    public void Build_WithCodeChallenge_AddsPkceParameters()
    {
        var url = BexioAuthorizeUrlBuilder.Build(Options, "state-1", codeChallenge: "challenge-1");
        var query = HttpUtility.ParseQueryString(url.Query);

        Assert.Multiple(() =>
        {
            Assert.That(query["code_challenge"], Is.EqualTo("challenge-1"));
            Assert.That(query["code_challenge_method"], Is.EqualTo("S256"));
        });
    }

    /// <summary>
    /// An explicit redirect URI overrides the configured default, for hosts serving several
    /// callbacks.
    /// </summary>
    [Test]
    public void Build_WithExplicitRedirectUri_OverridesOptions()
    {
        var url = BexioAuthorizeUrlBuilder.Build(Options, "state-1", "https://other.example.local/cb");

        HttpUtility.ParseQueryString(url.Query)["redirect_uri"].ShouldBe("https://other.example.local/cb");
    }

    /// <summary>
    /// Without a redirect URI there is nowhere to send the authorization code, so the call is
    /// rejected up front.
    /// </summary>
    [Test]
    public void Build_WithoutRedirectUri_Throws()
    {
        Assert.That(() => BexioAuthorizeUrlBuilder.Build(Options with { RedirectUri = null }, "state-1"),
            Throws.InstanceOf<ArgumentException>());
    }

    /// <summary>
    /// An anti-forgery state value is mandatory.
    /// </summary>
    [Test]
    public void Build_WithoutState_Throws()
    {
        Assert.That(() => BexioAuthorizeUrlBuilder.Build(Options, string.Empty), Throws.InstanceOf<ArgumentException>());
    }

    /// <summary>
    /// Endpoint derivation must not depend on whether the configured authority ends with a slash.
    /// </summary>
    [Test]
    public void Endpoints_AreDerivedFromAuthority_WithOrWithoutTrailingSlash()
    {
        var withSlash = Options with { Authority = "https://auth.example.local/realms/bexio/" };

        Assert.Multiple(() =>
        {
            Assert.That(Options.TokenEndpoint.ToString(),
                Is.EqualTo("https://auth.example.local/realms/bexio/protocol/openid-connect/token"));
            Assert.That(withSlash.TokenEndpoint, Is.EqualTo(Options.TokenEndpoint));
            Assert.That(withSlash.AuthorizationEndpoint, Is.EqualTo(Options.AuthorizationEndpoint));
        });
    }

    /// <summary>
    /// The default authority is the live bexio realm, so a consumer only has to supply client
    /// credentials.
    /// </summary>
    [Test]
    public void DefaultAuthority_PointsAtTheBexioRealm()
    {
        var defaults = new BexioOAuthOptions { ClientId = "client-id" };

        Assert.Multiple(() =>
        {
            Assert.That(defaults.Authority, Is.EqualTo("https://auth.bexio.com/realms/bexio"));
            Assert.That(defaults.TokenEndpoint.ToString(),
                Is.EqualTo("https://auth.bexio.com/realms/bexio/protocol/openid-connect/token"));
            Assert.That(defaults.ClockSkew, Is.EqualTo(TimeSpan.FromSeconds(60)));
        });
    }
}
