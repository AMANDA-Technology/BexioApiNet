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
/// Live end-to-end tests for the pagebreak-position connector exposed via
/// <see cref="IBexioApiClient.PagebreakPositions"/>. Tests are skipped when the required
/// environment variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are not
/// present. Mutating operations (create / update / delete) are intentionally omitted to avoid
/// leaving orphaned positions on the live tenant — they are covered offline by the integration
/// suite. Tests structurally validate the JSON payload deserializes into
/// <c>PositionPagebreak</c> per the OpenAPI <c>PositionPagebreakExtended</c> schema.
/// </summary>
[Category("E2E")]
public sealed class PagebreakPositionServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Verifies <see cref="IBexioApiClient.PagebreakPositions"/> is registered correctly via DI.
    /// </summary>
    [Test]
    public void PagebreakPositions_IsNotNull()
    {
        Assert.That(BexioApiClient, Is.Not.Null);
        Assert.That(BexioApiClient!.PagebreakPositions, Is.Not.Null);
    }

    /// <summary>
    /// Lists the pagebreak positions of the first available invoice and asserts the request
    /// round-trips successfully. Where positions exist, asserts each position has the
    /// <c>KbPositionPagebreak</c> discriminator per the OpenAPI schema.
    /// </summary>
    [Test]
    public async Task Get_ReturnsPagebreakPositions_FromInvoice()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var invoices = await BexioApiClient!.Invoices.Get();
        Assert.That(invoices.IsSuccess, Is.True);

        if (invoices.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var result = await BexioApiClient.PagebreakPositions.Get(KbDocumentType.Invoice, existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        if (result.Data is { Count: > 0 } positions)
        {
            Assert.Multiple(() =>
            {
                foreach (var position in positions)
                {
                    Assert.That(position.Type, Is.EqualTo("KbPositionPagebreak"));
                    Assert.That(position.Id, Is.Not.Null);
                }
            });
        }
    }

    /// <summary>
    /// Lists pagebreak positions for the first available quote and verifies the OpenAPI schema
    /// applies for the <c>kb_offer</c> document type as well.
    /// </summary>
    [Test]
    public async Task Get_ReturnsPagebreakPositions_FromQuote()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var quotes = await BexioApiClient!.Quotes.Get();
        Assert.That(quotes.IsSuccess, Is.True);

        if (quotes.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no quotes available on this tenant");
            return;
        }

        var result = await BexioApiClient.PagebreakPositions.Get(KbDocumentType.Offer, existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Lists pagebreak positions for the first available order and verifies the OpenAPI schema
    /// applies for the <c>kb_order</c> document type as well.
    /// </summary>
    [Test]
    public async Task Get_ReturnsPagebreakPositions_FromOrder()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var orders = await BexioApiClient!.Orders.Get();
        Assert.That(orders.IsSuccess, Is.True);

        if (orders.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no orders available on this tenant");
            return;
        }

        var result = await BexioApiClient.PagebreakPositions.Get(KbDocumentType.Order, existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
