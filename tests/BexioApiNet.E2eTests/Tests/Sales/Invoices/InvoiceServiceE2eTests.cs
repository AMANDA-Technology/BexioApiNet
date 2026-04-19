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

using BexioApiNet.Abstractions.Models.Api;
using BexioApiNet.Abstractions.Models.Sales.Invoices.Views;
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Sales.Invoices;

/// <summary>
/// Live end-to-end tests for the invoice connector exposed via
/// <see cref="IBexioApiClient.Invoices"/>. Tests are skipped when the required environment
/// variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are not present.
/// Mutating operations (create / update / cancel / delete) are intentionally omitted to avoid
/// leaving orphaned draft invoices on the live tenant — they are covered offline by the
/// integration suite.
/// </summary>
[Category("E2E")]
public sealed class InvoiceServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists the first page of invoices and asserts the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task Get_ReturnsInvoices()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Invoices.Get(new QueryParameterInvoice(Limit: 5));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Fetches a single invoice by id using the first one returned from the list endpoint
    /// and asserts round-trip equality on the id.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsInvoice()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var list = await BexioApiClient!.Invoices.Get(new QueryParameterInvoice(Limit: 1));
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var result = await BexioApiClient.Invoices.GetById(existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data?.Id, Is.EqualTo(existing[0].Id));
        });
    }

    /// <summary>
    /// Searches invoices by title and asserts the request round-trips successfully.
    /// </summary>
    [Test]
    public async Task Search_ReturnsInvoices()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "title", Value = "Test", Criteria = "like" }
        };

        var result = await BexioApiClient!.Invoices.Search(criteria);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Lists payments on the first available invoice and asserts the payment sub-resource
    /// route works end-to-end.
    /// </summary>
    [Test]
    public async Task GetPayments_ReturnsPayments()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var list = await BexioApiClient!.Invoices.Get(new QueryParameterInvoice(Limit: 1));
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is not { Count: > 0 } existing)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var result = await BexioApiClient.Invoices.GetPayments(existing[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
