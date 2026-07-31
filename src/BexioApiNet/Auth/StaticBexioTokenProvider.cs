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

namespace BexioApiNet.Auth;

/// <summary>
/// Supplies a pre-issued token that never changes, such as a Personal Access Token from
/// <see href="https://developer.bexio.com">developer.bexio.com</see>. This is the provider behind
/// the <c>jwtToken</c> based registration overloads.
/// </summary>
/// <remarks>
/// A Personal Access Token expires after 60 days, carries all default scopes, and bexio documents
/// it as "strictly intended for personal use and should never be shared". It is therefore a poor
/// fit for an unattended server integration — use
/// <see cref="RefreshTokenBexioTokenProvider" /> with the authorization code flow instead.
/// </remarks>
public sealed class StaticBexioTokenProvider : IBexioTokenProvider
{
    private readonly string _accessToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticBexioTokenProvider" /> class.
    /// </summary>
    /// <param name="accessToken">Pre-issued bearer token.</param>
    public StaticBexioTokenProvider(string accessToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        _accessToken = accessToken;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Always false: a rejected pre-issued token stays rejected, so
    /// <see cref="BexioAuthDelegatingHandler" /> skips its retry for this provider.
    /// </remarks>
    public bool CanRenew => false;

    /// <inheritdoc />
    public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_accessToken);

    /// <inheritdoc />
    /// <remarks>
    /// No-op: there is no cache to clear and nothing to renew.
    /// </remarks>
    public void Invalidate(string accessToken)
    {
    }
}
