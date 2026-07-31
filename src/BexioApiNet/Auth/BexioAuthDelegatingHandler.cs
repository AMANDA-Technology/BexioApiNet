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
using System.Net.Http.Headers;

namespace BexioApiNet.Auth;

/// <summary>
/// Sets the <c>Authorization</c> header on every outgoing bexio request from an
/// <see cref="IBexioTokenProvider" />. Resolving the token per request — rather than pinning it to
/// <see cref="HttpClient.DefaultRequestHeaders" /> — is what lets a short-lived OIDC access token
/// be renewed without recreating the client.
/// </summary>
/// <remarks>
/// A <c>401</c> is retried exactly once: the cached token is invalidated, a fresh one is fetched
/// and the request is replayed. If the provider hands back the same token (a static Personal
/// Access Token, for example) the retry is skipped, because it would fail identically.
/// </remarks>
public sealed class BexioAuthDelegatingHandler : DelegatingHandler
{
    private readonly IBexioTokenProvider _tokenProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioAuthDelegatingHandler" /> class. Used by
    /// <c>IHttpClientFactory</c>, which supplies the inner handler.
    /// </summary>
    /// <param name="tokenProvider">Provider of the bearer token.</param>
    public BexioAuthDelegatingHandler(IBexioTokenProvider tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        _tokenProvider = tokenProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioAuthDelegatingHandler" /> class with an
    /// explicit inner handler, for callers building their own pipeline.
    /// </summary>
    /// <param name="tokenProvider">Provider of the bearer token.</param>
    /// <param name="innerHandler">Next handler in the pipeline.</param>
    public BexioAuthDelegatingHandler(IBexioTokenProvider tokenProvider, HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        _tokenProvider = tokenProvider;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Buffer the body up-front so a retry can replay it — a streamed content can only be read once.
        if (request.Content is not null)
            await request.Content.LoadIntoBufferAsync(cancellationToken);

        var accessToken = await _tokenProvider.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue(BexioAuthDefaults.BearerScheme, accessToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        _tokenProvider.Invalidate();
        var renewedToken = await _tokenProvider.GetAccessTokenAsync(cancellationToken);

        if (string.Equals(renewedToken, accessToken, StringComparison.Ordinal))
            return response;

        response.Dispose();

        using var retry = await CloneRequestAsync(request, cancellationToken);
        retry.Headers.Authorization = new AuthenticationHeaderValue(BexioAuthDefaults.BearerScheme, renewedToken);

        return await base.SendAsync(retry, cancellationToken);
    }

    /// <summary>
    /// Copies a sent request so it can be sent again. An <see cref="HttpRequestMessage" /> cannot
    /// be reused, and its content stream has already been consumed — the body is taken from the
    /// buffer prepared before the first send.
    /// </summary>
    /// <param name="request">The request to copy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An unsent copy of the request.</returns>
    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version, VersionPolicy = request.VersionPolicy };

        if (request.Content is not null)
        {
            clone.Content = new ByteArrayContent(await request.Content.ReadAsByteArrayAsync(cancellationToken));

            foreach (var (name, values) in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(name, values);
        }

        foreach (var (name, values) in request.Headers)
            clone.Headers.TryAddWithoutValidation(name, values);

        foreach (var (key, value) in request.Options)
            clone.Options.Set(new HttpRequestOptionsKey<object?>(key), value);

        return clone;
    }
}
