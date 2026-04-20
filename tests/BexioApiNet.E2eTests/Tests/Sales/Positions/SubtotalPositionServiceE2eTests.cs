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

namespace BexioApiNet.E2eTests.Tests.Sales.Positions;

/// <summary>
/// Live end-to-end tests for subtotal positions exposed via
/// <see cref="IBexioApiClient.SalesSubtotalPositions"/>. Tests are skipped when the required
/// environment variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are absent.
/// Mutating operations (create / update / delete) are intentionally omitted to avoid leaving
/// orphaned positions on the live tenant — they are covered offline by the integration suite.
/// </summary>
[Category("E2E")]
public sealed class SubtotalPositionServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Verifies that <see cref="IBexioApiClient.SalesSubtotalPositions"/> is non-null after
    /// construction — confirms the service is wired up correctly via DI.
    /// </summary>
    [Test]
    public void SalesSubtotalPositions_IsNotNull()
    {
        Assert.That(BexioApiClient, Is.Not.Null);
        Assert.That(BexioApiClient!.SalesSubtotalPositions, Is.Not.Null);
    }

    /// <summary>
    /// Lists subtotal positions for the first available quote and asserts the request round-trips
    /// successfully. Skipped when no quotes exist on the tenant.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsSubtotalPositions()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var quotes = await BexioApiClient!.Quotes.Get();
        Assert.That(quotes.IsSuccess, Is.True);

        if (quotes.Data is not { Count: > 0 } existingQuotes)
        {
            Assert.Ignore("no quotes available on this tenant");
            return;
        }

        var result = await BexioApiClient.SalesSubtotalPositions.GetAll("kb_offer", existingQuotes[0].Id!);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
        });
    }
}
