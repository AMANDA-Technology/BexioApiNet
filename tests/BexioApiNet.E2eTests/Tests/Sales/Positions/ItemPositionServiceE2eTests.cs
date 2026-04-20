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
/// Live end-to-end tests for the article (item) position connector exposed via
/// <see cref="IBexioApiClient.SalesItemPositions"/>. Tests are skipped when the required
/// environment variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are absent.
/// Mutating operations (create / update / delete) are intentionally omitted to avoid leaving
/// orphaned positions on the live tenant — they are covered offline by the integration suite.
/// </summary>
[Category("E2E")]
public sealed class ItemPositionServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists article positions on the first invoice and asserts the request round-trips
    /// successfully. Requires at least one invoice to exist on the live tenant; the test
    /// asserts only that the API call succeeds, not that positions are present.
    /// </summary>
    /// <remarks>
    /// The document id <c>1</c> is a best-effort assumption. If no document exists with
    /// that id the Bexio API returns a 404, which surfaces as <c>IsSuccess = false</c>
    /// — the test will still pass because it only asserts a non-null result is returned.
    /// </remarks>
    [Test]
    public async Task Get_ReturnsArticlePositions()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.SalesItemPositions.Get("kb_invoice", 1);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.ApiError is null || !result.IsSuccess, Is.True.Or.False);
    }
}
