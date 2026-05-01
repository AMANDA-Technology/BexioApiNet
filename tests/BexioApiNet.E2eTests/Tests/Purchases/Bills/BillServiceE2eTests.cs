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

using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Purchases.Bills;

/// <summary>
/// Live E2E tests for the <see cref="BexioApiNet.Services.Connectors.Purchases.BillService"/>.
/// Read-only smoke tests against the live tenant: listing bills (paged envelope), fetching a
/// bill by id (full schema round-trip including <c>line_items</c>, <c>discounts</c>, <c>address</c>,
/// optional <c>payment</c> block) and validating a document number. Tests are auto-skipped when
/// credentials are missing per <see cref="BexioE2eTestBase"/>. Mutating Create/Update/Delete
/// flows are intentionally not exercised here — bills affect accounting state and cannot be
/// safely created and torn down inside an arbitrary test tenant.
/// </summary>
[Category("E2E")]
public sealed class BillServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists bills and asserts the response envelope deserializes correctly —
    /// <c>data</c> and <c>paging</c> must both be populated.
    /// </summary>
    [Test]
    public async Task Get_ReturnsListResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.PurchaseBills.Get();

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ApiError, Is.Null);
        Assert.That(result.Data, Is.Not.Null);

        Assert.Multiple(() =>
        {
            Assert.That(result.Data!.Data, Is.Not.Null);
            Assert.That(result.Data.Paging, Is.Not.Null);
            Assert.That(result.Data.Paging.Page, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Data.Paging.PageSize, Is.GreaterThan(0));
        });
    }

    /// <summary>
    /// Fetches the first bill returned by the list endpoint (when any exist) and
    /// asserts the full bill payload deserializes through
    /// <see cref="BexioApiNet.Services.Connectors.Purchases.BillService.GetById"/>.
    /// Verifies both the round-trip identity and that the required fields documented
    /// in the v3.0.0 OpenAPI <c>Bill</c> schema are populated.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsBillOrIgnoresWhenNoneExist()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var listResult = await BexioApiClient!.PurchaseBills.Get(new QueryParameterBill(limit: 1));

        Assert.That(listResult.IsSuccess, Is.True);

        if (listResult.Data?.Data.Count is not > 0)
        {
            Assert.Ignore("no bills available in the target Bexio account");
            return;
        }

        var firstId = listResult.Data.Data[0].Id;

        var result = await BexioApiClient.PurchaseBills.GetById(firstId);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ApiError, Is.Null);
        Assert.That(result.Data, Is.Not.Null);

        var bill = result.Data!;
        Assert.Multiple(() =>
        {
            Assert.That(bill.Id, Is.EqualTo(firstId));
            Assert.That(bill.DocumentNo, Is.Not.Null.And.Not.Empty);
            Assert.That(bill.LastnameCompany, Is.Not.Null);
            Assert.That(bill.SupplierId, Is.GreaterThan(0));
            Assert.That(bill.ContactPartnerId, Is.GreaterThan(0));
            Assert.That(bill.BaseCurrencyCode, Is.Not.Null.And.Not.Empty);
            Assert.That(bill.Address, Is.Not.Null);
            Assert.That(bill.LineItems, Is.Not.Null);
            Assert.That(bill.Discounts, Is.Not.Null);
            Assert.That(bill.AttachmentIds, Is.Not.Null);
        });
    }

    /// <summary>
    /// Validates a proposed document number. For an arbitrary candidate the endpoint
    /// should respond with a successful result — <c>valid</c> may be <c>true</c> or
    /// <c>false</c> depending on the account state, so the test only asserts the call
    /// succeeds and the response deserializes through <c>BillDocumentNumberResponse</c>.
    /// </summary>
    [Test]
    public async Task GetDocNumbers_ReturnsValidationResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.PurchaseBills.GetDocNumbers("BEXIOAPINET-E2E-PROBE");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
