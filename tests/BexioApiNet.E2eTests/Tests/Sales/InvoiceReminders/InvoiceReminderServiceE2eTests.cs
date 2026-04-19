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
using BexioApiNet.Models;

namespace BexioApiNet.E2eTests.Tests.Sales.InvoiceReminders;

/// <summary>
/// Live end-to-end tests for the invoice reminder connector exposed via
/// <see cref="IBexioApiClient.InvoiceReminders"/>. Tests are skipped when the required
/// environment variables (<c>BexioApiNet__BaseUri</c>, <c>BexioApiNet__JwtToken</c>) are not
/// present. Mutating operations (create / send / mark / delete) are intentionally omitted to
/// avoid leaving orphaned reminders on the live tenant — they are covered offline by the
/// integration and unit suites.
/// </summary>
[Category("E2E")]
public sealed class InvoiceReminderServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists reminders for the first available invoice and asserts the nested route round-trips
    /// successfully. The test is skipped when the tenant has no invoices.
    /// </summary>
    [Test]
    public async Task Get_ReturnsReminders()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var list = await BexioApiClient!.Invoices.Get(new QueryParameterInvoice(Limit: 1));
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is not { Count: > 0 } existingInvoices)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var result = await BexioApiClient.InvoiceReminders.Get(existingInvoices[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }

    /// <summary>
    /// Fetches a single reminder by id using the first one returned from the list endpoint
    /// and asserts round-trip equality on the id. Skipped when no reminder exists on the
    /// first available invoice.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsReminder()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var invoiceList = await BexioApiClient!.Invoices.Get(new QueryParameterInvoice(Limit: 1));
        Assert.That(invoiceList.IsSuccess, Is.True);

        if (invoiceList.Data is not { Count: > 0 } existingInvoices)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var invoiceId = existingInvoices[0].Id;

        var reminderList = await BexioApiClient.InvoiceReminders.Get(invoiceId);
        Assert.That(reminderList.IsSuccess, Is.True);

        if (reminderList.Data is not { Count: > 0 } existingReminders)
        {
            Assert.Ignore("no reminders available for the first invoice on this tenant");
            return;
        }

        var result = await BexioApiClient.InvoiceReminders.GetById(invoiceId, existingReminders[0].Id);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data?.Id, Is.EqualTo(existingReminders[0].Id));
            Assert.That(result.Data?.KbInvoiceId, Is.EqualTo(invoiceId));
        });
    }

    /// <summary>
    /// Searches reminders for the first available invoice by title and asserts the request
    /// round-trips successfully. Skipped when no invoice is available.
    /// </summary>
    [Test]
    public async Task Search_ReturnsReminders()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var invoiceList = await BexioApiClient!.Invoices.Get(new QueryParameterInvoice(Limit: 1));
        Assert.That(invoiceList.IsSuccess, Is.True);

        if (invoiceList.Data is not { Count: > 0 } existingInvoices)
        {
            Assert.Ignore("no invoices available on this tenant");
            return;
        }

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "title", Value = "Test", Criteria = "like" }
        };

        var result = await BexioApiClient.InvoiceReminders.Search(existingInvoices[0].Id, criteria);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
