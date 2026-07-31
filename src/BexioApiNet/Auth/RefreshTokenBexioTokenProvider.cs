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
/// Mints access tokens from a stored refresh token, the unattended path for an integration that
/// completed the authorization code flow with the <see cref="BexioAuthDefaults.OfflineAccessScope" />
/// scope.
/// </summary>
/// <remarks>
/// bexio rotates refresh tokens: every refresh invalidates the token it was called with and
/// returns a replacement. This provider persists the replacement through
/// <see cref="IBexioRefreshTokenStore" /> and only then reports the refresh as successful. If the
/// store write fails the exception propagates and no access token is cached, so the caller learns
/// about the failure instead of running on a token whose refresh credential was silently lost.
/// </remarks>
public sealed class RefreshTokenBexioTokenProvider : CachingBexioTokenProvider
{
    private readonly IBexioTokenClient _tokenClient;
    private readonly IBexioRefreshTokenStore _refreshTokenStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenBexioTokenProvider" /> class.
    /// </summary>
    /// <param name="tokenClient">Client for the bexio token endpoint.</param>
    /// <param name="refreshTokenStore">Persistence for the rotating refresh token.</param>
    /// <param name="options">Client registration and endpoint settings.</param>
    /// <param name="timeProvider">Time source. Defaults to <see cref="TimeProvider.System" />.</param>
    public RefreshTokenBexioTokenProvider(IBexioTokenClient tokenClient, IBexioRefreshTokenStore refreshTokenStore,
        BexioOAuthOptions options, TimeProvider? timeProvider = null)
        : base(GetClockSkew(options), timeProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenClient);
        ArgumentNullException.ThrowIfNull(refreshTokenStore);

        _tokenClient = tokenClient;
        _refreshTokenStore = refreshTokenStore;
    }

    /// <inheritdoc />
    protected override async Task<BexioTokenResponse> AcquireTokenAsync(CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenStore.GetRefreshTokenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new BexioAuthenticationException(
                "No refresh token is stored. Complete the bexio authorization code flow with the "
                + $"'{BexioAuthDefaults.OfflineAccessScope}' scope and persist the issued refresh token first.");

        var response = await _tokenClient.RefreshTokenAsync(refreshToken, cancellationToken);

        // The rotated token must reach the store before this refresh counts as successful — bexio
        // has already invalidated the old one at this point.
        if (!string.IsNullOrWhiteSpace(response.RefreshToken) && !string.Equals(response.RefreshToken, refreshToken, StringComparison.Ordinal))
            await _refreshTokenStore.StoreRefreshTokenAsync(response.RefreshToken, cancellationToken);

        return response;
    }

    /// <summary>
    /// Reads the clock skew from the options, guarding against a null argument before the base
    /// constructor runs.
    /// </summary>
    /// <param name="options">Client registration and endpoint settings.</param>
    private static TimeSpan GetClockSkew(BexioOAuthOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ClockSkew;
    }
}
