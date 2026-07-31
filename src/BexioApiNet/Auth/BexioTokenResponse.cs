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

using System.Text.Json.Serialization;

namespace BexioApiNet.Auth;

/// <summary>
/// Successful response of the bexio token endpoint.
/// </summary>
public sealed record BexioTokenResponse
{
    /// <summary>
    /// Access token to send as bearer token to the bexio API.
    /// </summary>
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    /// <summary>
    /// Token type, always <c>Bearer</c> for bexio.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }

    /// <summary>
    /// Lifetime of <see cref="AccessToken" /> in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Refresh token. Issued when <see cref="BexioAuthDefaults.OfflineAccessScope" /> was
    /// requested. bexio rotates refresh tokens, so a value returned here replaces the one used
    /// for the request.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Lifetime of <see cref="RefreshToken" /> in seconds. <c>0</c> means it does not expire.
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; init; }

    /// <summary>
    /// Space separated list of scopes actually granted.
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    /// <summary>
    /// OpenID Connect id token, when the <c>openid</c> scope was requested.
    /// </summary>
    [JsonPropertyName("id_token")]
    public string? IdToken { get; init; }
}
