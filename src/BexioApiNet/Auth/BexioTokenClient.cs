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

using System.Net;
using System.Text.Json;

namespace BexioApiNet.Auth;

/// <inheritdoc />
public sealed class BexioTokenClient : IBexioTokenClient
{
    private readonly BexioOAuthOptions _options;

    /// <summary>
    /// Produces the client used for a single token request.
    /// </summary>
    private readonly Func<HttpClient> _httpClientFactory;

    /// <summary>
    /// True when <see cref="_httpClientFactory" /> hands out a fresh client per request that this
    /// instance must dispose. False when the same externally owned client is reused.
    /// </summary>
    private readonly bool _disposeClientPerRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioTokenClient" /> class using an externally
    /// managed <see cref="HttpClient" />, which this instance never disposes.
    /// </summary>
    /// <param name="httpClient">Client used for token endpoint calls.</param>
    /// <param name="options">Client registration and endpoint settings.</param>
    public BexioTokenClient(HttpClient httpClient, BexioOAuthOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientId);

        _httpClientFactory = () => httpClient;
        _disposeClientPerRequest = false;
        _options = options;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioTokenClient" /> class that obtains a
    /// client per request from the supplied factory. Intended for DI, where the factory delegates
    /// to <c>IHttpClientFactory</c> so a long-lived token client does not pin a single handler.
    /// </summary>
    /// <param name="httpClientFactory">Factory producing a client per token request.</param>
    /// <param name="options">Client registration and endpoint settings.</param>
    public BexioTokenClient(Func<HttpClient> httpClientFactory, BexioOAuthOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ClientId);

        _httpClientFactory = httpClientFactory;
        _disposeClientPerRequest = true;
        _options = options;
    }

    /// <inheritdoc />
    public Task<BexioTokenResponse> ExchangeAuthorizationCodeAsync(string code, string? redirectUri = null,
        string? codeVerifier = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var effectiveRedirectUri = redirectUri ?? _options.RedirectUri;
        ArgumentException.ThrowIfNullOrWhiteSpace(effectiveRedirectUri);

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = effectiveRedirectUri
        };

        if (!string.IsNullOrWhiteSpace(codeVerifier))
            parameters["code_verifier"] = codeVerifier;

        return RequestTokenAsync(parameters, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BexioTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        return RequestTokenAsync(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BexioTokenResponse> ClientCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        };

        if (_options.Scopes.Count > 0)
            parameters["scope"] = string.Join(' ', _options.Scopes);

        return RequestTokenAsync(parameters, cancellationToken);
    }

    /// <summary>
    /// Posts a token request as <c>application/x-www-form-urlencoded</c>. Client credentials are
    /// added to the body rather than the query string, which the current bexio identity provider
    /// requires.
    /// </summary>
    /// <param name="parameters">Grant specific parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued tokens.</returns>
    private async Task<BexioTokenResponse> RequestTokenAsync(Dictionary<string, string> parameters, CancellationToken cancellationToken)
    {
        parameters["client_id"] = _options.ClientId;

        if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
            parameters["client_secret"] = _options.ClientSecret;

        var httpClient = _httpClientFactory();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(parameters)
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw CreateFailure(response.StatusCode, body);

            var token = TryDeserialize<BexioTokenResponse>(body);

            return token is null || string.IsNullOrWhiteSpace(token.AccessToken)
                ? throw new BexioAuthenticationException(
                    $"The bexio token endpoint returned {(int)response.StatusCode} without a usable access token.")
                : token;
        }
        finally
        {
            if (_disposeClientPerRequest)
                httpClient.Dispose();
        }
    }

    /// <summary>
    /// Builds the exception for a rejected token request. Only the OAuth error fields are copied
    /// into the message — the request body carries the client secret and must never be echoed.
    /// </summary>
    /// <param name="statusCode">Status code returned by the token endpoint.</param>
    /// <param name="body">Raw response body.</param>
    private static BexioAuthenticationException CreateFailure(HttpStatusCode statusCode, string body)
    {
        var error = TryDeserialize<BexioTokenErrorResponse>(body);
        var detail = string.IsNullOrWhiteSpace(error?.Error)
            ? string.Empty
            : $": {error.Error}{(string.IsNullOrWhiteSpace(error.ErrorDescription) ? string.Empty : $" - {error.ErrorDescription}")}";

        return new BexioAuthenticationException(
            $"The bexio token endpoint returned {(int)statusCode}{detail}.",
            statusCode,
            error?.Error,
            error?.ErrorDescription);
    }

    /// <summary>
    /// Deserializes a token endpoint body, returning <c>default</c> for empty or non-JSON payloads
    /// (for example an HTML error page from a proxy in front of the identity provider).
    /// </summary>
    /// <param name="content">The response body.</param>
    /// <typeparam name="T">Target deserialization type.</typeparam>
    private static T? TryDeserialize<T>(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(content);
        }
        catch (JsonException)
        {
            return default;
        }
    }
}
