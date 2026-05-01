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
using BexioApiNet.Abstractions.Models.Sales.Positions.Views;

namespace BexioApiNet.E2eTests.Tests.Sales.Positions;

/// <summary>
/// Live end-to-end tests for the subtotal-position connector exposed via
/// <see cref="IBexioApiClient.SalesSubtotalPositions"/>. Tests are skipped when the required
/// environment variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are not
/// present. The CRUD lifecycle test is opt-in: it picks the first available quote on the
/// tenant, exercises Create → Read → Update → Delete against it and always cleans up the
/// created subtotal in a <c>finally</c> block — even if intermediate assertions fail — to
/// avoid leaking state on the live tenant. Tests structurally validate the JSON payload
/// deserializes into <c>PositionSubtotal</c> per the OpenAPI <c>PositionSubtotalExtended</c>
/// schema. Subtotal positions are not valid on deliveries per the spec.
/// </summary>
[Category("E2E")]
public sealed class SubtotalPositionServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Verifies <see cref="IBexioApiClient.SalesSubtotalPositions"/> is registered correctly via DI.
    /// </summary>
    [Test]
    public void SalesSubtotalPositions_IsNotNull()
    {
        Assert.That(BexioApiClient, Is.Not.Null);
        Assert.That(BexioApiClient!.SalesSubtotalPositions, Is.Not.Null);
    }

    /// <summary>
    /// Lists the subtotal positions of the first available invoice and asserts the request
    /// round-trips successfully. Where positions exist, asserts each has the
    /// <c>KbPositionSubtotal</c> discriminator per the OpenAPI schema.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsSubtotalPositions_FromInvoice()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var invoices = await BexioApiClient!.Invoices.Get();
        Assert.That(invoices.IsSuccess, Is.True);

        if (invoices.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var result = await BexioApiClient.SalesSubtotalPositions.GetAll(KbDocumentType.Invoice, existing[0].Id);

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
                    Assert.That(position.Type, Is.EqualTo("KbPositionSubtotal"));
                    Assert.That(position.Id, Is.Not.Null);
                }
            });
        }
    }

    /// <summary>
    /// Lists subtotal positions for the first available quote and verifies the OpenAPI schema
    /// applies for the <c>kb_offer</c> document type as well.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsSubtotalPositions_FromQuote()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var quotes = await BexioApiClient!.Quotes.Get();
        Assert.That(quotes.IsSuccess, Is.True);

        if (quotes.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no quotes available on this tenant");
            return;
        }

        var result = await BexioApiClient.SalesSubtotalPositions.GetAll(KbDocumentType.Offer, existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Lists subtotal positions for the first available order and verifies the OpenAPI schema
    /// applies for the <c>kb_order</c> document type as well.
    /// </summary>
    [Test]
    public async Task GetAll_ReturnsSubtotalPositions_FromOrder()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var orders = await BexioApiClient!.Orders.Get();
        Assert.That(orders.IsSuccess, Is.True);

        if (orders.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no orders available on this tenant");
            return;
        }

        var result = await BexioApiClient.SalesSubtotalPositions.GetAll(KbDocumentType.Order, existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Full Create → Read → Update → Delete lifecycle against the first available quote on the
    /// tenant. Skipped when no quote exists. The created subtotal position is always cleaned up —
    /// even if intermediate assertions fail — to avoid leaking state on the live tenant.
    /// </summary>
    [Test]
    public async Task SubtotalPosition_Lifecycle_CreateReadUpdateDelete()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var quotes = await BexioApiClient!.Quotes.Get();
        Assert.That(quotes.IsSuccess, Is.True);

        if (quotes.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no quotes available on this tenant — cannot run lifecycle test");
            return;
        }

        var quoteId = existing[0].Id;
        int? createdPositionId = null;

        try
        {
            var createPayload = new PositionSubtotalCreate(Text: "E2E test running total");
            var created = await BexioApiClient.SalesSubtotalPositions.Create(KbDocumentType.Offer, quoteId, createPayload);

            Assert.Multiple(() =>
            {
                Assert.That(created.IsSuccess, Is.True);
                Assert.That(created.ApiError, Is.Null);
                Assert.That(created.Data, Is.Not.Null);
                Assert.That(created.Data!.Type, Is.EqualTo("KbPositionSubtotal"));
                Assert.That(created.Data.Text, Is.EqualTo("E2E test running total"));
            });

            createdPositionId = created.Data!.Id;
            Assert.That(createdPositionId, Is.Not.Null);

            var read = await BexioApiClient.SalesSubtotalPositions.GetById(KbDocumentType.Offer, quoteId, createdPositionId!.Value);
            Assert.Multiple(() =>
            {
                Assert.That(read.IsSuccess, Is.True);
                Assert.That(read.Data, Is.Not.Null);
                Assert.That(read.Data!.Id, Is.EqualTo(createdPositionId));
                Assert.That(read.Data.Text, Is.EqualTo("E2E test running total"));
            });

            var updatePayload = new PositionSubtotalCreate(Text: "E2E updated running total");
            var updated = await BexioApiClient.SalesSubtotalPositions.Update(KbDocumentType.Offer, quoteId, createdPositionId.Value, updatePayload);
            Assert.Multiple(() =>
            {
                Assert.That(updated.IsSuccess, Is.True);
                Assert.That(updated.Data, Is.Not.Null);
                Assert.That(updated.Data!.Text, Is.EqualTo("E2E updated running total"));
            });
        }
        finally
        {
            if (createdPositionId is not null)
            {
                var deleted = await BexioApiClient.SalesSubtotalPositions.Delete(KbDocumentType.Offer, quoteId, createdPositionId.Value);
                Assert.That(deleted.IsSuccess, Is.True);
            }
        }
    }
}
