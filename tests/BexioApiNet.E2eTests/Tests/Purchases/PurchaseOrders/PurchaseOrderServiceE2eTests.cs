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

namespace BexioApiNet.E2eTests.Tests.Purchases.PurchaseOrders;

/// <summary>
/// Live E2E tests for the <see cref="BexioApiNet.Services.Connectors.Purchases.PurchaseOrderService"/>.
/// Read-only calls only: listing purchase orders and retrieving a purchase order by id.
/// Tests are auto-skipped when credentials are missing per <see cref="BexioE2eTestBase"/>.
/// </summary>
[Category("E2E")]
public sealed class PurchaseOrderServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists purchase orders and asserts the list response deserializes correctly.
    /// </summary>
    [Test]
    public async Task Get_ReturnsListResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.PurchaseOrders.Get();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Fetches the first purchase order returned by the list endpoint (when any exist) and
    /// asserts the full payload deserializes correctly through
    /// <see cref="BexioApiNet.Services.Connectors.Purchases.PurchaseOrderService.GetById"/>.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsPurchaseOrderOrIgnoresWhenNoneExist()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var listResult = await BexioApiClient!.PurchaseOrders.Get();

        Assert.That(listResult.IsSuccess, Is.True);

        if (listResult.Data is not { Count: > 0 })
        {
            Assert.Ignore("no purchase orders available in the target Bexio account");
            return;
        }

        var firstId = listResult.Data[0].Id;

        var result = await BexioApiClient.PurchaseOrders.GetById(firstId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(firstId));
        });
    }
}
