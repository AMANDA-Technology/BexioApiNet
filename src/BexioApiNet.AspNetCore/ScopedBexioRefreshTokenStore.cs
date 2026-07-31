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

using BexioApiNet.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace BexioApiNet.AspNetCore;

/// <summary>
/// Resolves the host's <see cref="IBexioRefreshTokenStore" /> from a fresh scope for every call.
/// The token provider is a singleton, so it cannot hold a scoped store directly — and a store
/// backed by a database context is naturally scoped.
/// </summary>
/// <param name="scopeFactory">Factory used to create a scope per store operation.</param>
internal sealed class ScopedBexioRefreshTokenStore(IServiceScopeFactory scopeFactory) : IBexioRefreshTokenStore
{
    /// <inheritdoc />
    public async Task<string?> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IBexioRefreshTokenStore>()
            .GetRefreshTokenAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task StoreRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IBexioRefreshTokenStore>()
            .StoreRefreshTokenAsync(refreshToken, cancellationToken);
    }
}
