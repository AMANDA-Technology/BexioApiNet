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
/// Persistence for the rotating bexio refresh token. Implemented by the host — this library
/// intentionally ships no concrete store, because the storage technology (database, secret
/// manager, file) is an application decision.
/// </summary>
/// <remarks>
/// The refresh token is a long-lived credential granting access to the customer's bexio company.
/// Store it encrypted at rest and never log it.
/// </remarks>
public interface IBexioRefreshTokenStore
{
    /// <summary>
    /// Loads the current refresh token, or null when the application has not completed the
    /// authorization code flow yet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored refresh token, or null.</returns>
    Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a rotated refresh token, replacing the previous one.
    /// </summary>
    /// <remarks>
    /// This must not return before the value is durably stored. bexio invalidates the previous
    /// refresh token as soon as it issues a replacement, so a lost write leaves the integration
    /// with no usable credential and requires the customer to re-consent.
    /// </remarks>
    /// <param name="refreshToken">The new refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StoreRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
