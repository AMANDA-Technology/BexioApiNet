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

using BexioApiNet.Abstractions.Enums.Sales;

namespace BexioApiNet.E2eTests.Tests.Sales.Positions;

/// <summary>
/// Live end-to-end tests for the sub-position connector exposed via
/// <see cref="IBexioApiClient.SubPositions"/>. Tests are skipped when the required environment
/// variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are not present.
/// Mutating operations (create / update / delete) are intentionally omitted to avoid leaving
/// orphaned positions on the live tenant — they are covered offline by the integration suite.
/// </summary>
[Category("E2E")]
public sealed class SubPositionServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists the sub-positions of the first available invoice and asserts the request
    /// round-trips successfully. Skips gracefully when no invoices exist on the tenant.
    /// </summary>
    [Test]
    public async Task Get_ReturnsSubPositions()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var invoices = await BexioApiClient!.Invoices.Get();
        Assert.That(invoices.IsSuccess, Is.True);

        if (invoices.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var result = await BexioApiClient.SubPositions.Get(KbDocumentType.Invoice, existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
