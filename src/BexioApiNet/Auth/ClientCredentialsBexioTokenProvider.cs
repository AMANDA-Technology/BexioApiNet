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
/// Mints access tokens with the <c>client_credentials</c> grant. Needs no user consent and no
/// stored refresh token, but only works if the bexio app registration permits the grant — see
/// <c>doc/analysis/api-doc-discrepancies.md</c>.
/// </summary>
public sealed class ClientCredentialsBexioTokenProvider : CachingBexioTokenProvider
{
    private readonly IBexioTokenClient _tokenClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientCredentialsBexioTokenProvider" /> class.
    /// </summary>
    /// <param name="tokenClient">Client for the bexio token endpoint.</param>
    /// <param name="options">Client registration and endpoint settings.</param>
    /// <param name="timeProvider">Time source. Defaults to <see cref="TimeProvider.System" />.</param>
    public ClientCredentialsBexioTokenProvider(IBexioTokenClient tokenClient, BexioOAuthOptions options,
        TimeProvider? timeProvider = null)
        : base(GetClockSkew(options), timeProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenClient);
        _tokenClient = tokenClient;
    }

    /// <inheritdoc />
    protected override Task<BexioTokenResponse> AcquireTokenAsync(CancellationToken cancellationToken)
        => _tokenClient.ClientCredentialsAsync(cancellationToken);

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
