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

namespace BexioApiNet.E2eTests.Tests.Expenses.Expenses;

/// <summary>
/// Live E2E tests for the <see cref="BexioApiNet.Services.Connectors.Expenses.ExpenseService"/>
/// against Bexio v3 OpenAPI <c>/4.0/expenses</c>. Tests are auto-skipped when credentials
/// are missing per <see cref="BexioE2eTestBase"/>.
/// <para>
/// Read-only flows: list, get-by-id, document-number validation. Write flows
/// (Create/Update/UpdateBookings/Delete/Actions) are not run live because the spec
/// requires <c>attachment_ids</c> on Create — uploading a file end-to-end is out of scope
/// for this fixture and would leave artifacts in the tenant. Those code paths are covered
/// in the unit and integration test suites against WireMock stubs.
/// </para>
/// </summary>
[Category("E2E")]
public sealed class ExpenseServiceE2eTests : BexioE2eTestBase
{
    /// <summary>
    /// Lists expenses and asserts the response envelope deserializes correctly —
    /// <c>data</c> and <c>paging</c> must both be populated and each list item must
    /// satisfy the schema-required fields.
    /// </summary>
    [Test]
    public async Task Get_ReturnsListResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Expenses.Get();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Data, Is.Not.Null);
            Assert.That(result.Data.Paging, Is.Not.Null);
        });

        if (result.Data is { Data.Count: > 0 })
        {
            foreach (var item in result.Data.Data)
            {
                Assert.That(item.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(item.DocumentNo, Is.Not.Null.And.Not.Empty);
                Assert.That(item.CurrencyCode, Is.Not.Null.And.Not.Empty);
                Assert.That(item.AttachmentIds, Is.Not.Null);
            }
        }
    }

    /// <summary>
    /// Verifies that paging query parameters reach the URL and the response envelope
    /// reflects the requested page size (when expenses exist).
    /// </summary>
    [Test]
    public async Task Get_WithPagingParameters_ReturnsRequestedPageSize()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Expenses.Get(new QueryParameterExpense(limit: 5, page: 1));

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Paging.Page, Is.EqualTo(1));
            Assert.That(result.Data.Paging.PageSize, Is.LessThanOrEqualTo(5));
        });
    }

    /// <summary>
    /// Fetches the first expense returned by the list endpoint (when any exist) and
    /// asserts the full expense payload deserializes correctly through
    /// <see cref="BexioApiNet.Services.Connectors.Expenses.ExpenseService.GetById"/>.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsExpenseOrIgnoresWhenNoneExist()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var listResult = await BexioApiClient!.Expenses.Get(new QueryParameterExpense(limit: 1));

        Assert.That(listResult.IsSuccess, Is.True);

        if (listResult.Data?.Data.Count is not > 0)
        {
            Assert.Ignore("no expenses available in the target Bexio account");
            return;
        }

        var firstId = listResult.Data.Data[0].Id;

        var result = await BexioApiClient.Expenses.GetById(firstId);

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(firstId));
            Assert.That(result.Data.CurrencyCode, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Data.BaseCurrencyCode, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Data.AttachmentIds, Is.Not.Null);
        });
    }

    /// <summary>
    /// Validates a proposed document number. For an arbitrary candidate the endpoint
    /// should respond with a successful result — <c>valid</c> may be <c>true</c> or
    /// <c>false</c> depending on the account state, so the test only asserts that the
    /// call succeeds and the response deserializes.
    /// </summary>
    [Test]
    public async Task GetDocNumbers_ReturnsValidationResponse()
    {
        Assert.That(BexioApiClient, Is.Not.Null);

        var result = await BexioApiClient!.Expenses.GetDocNumbers("BEXIOAPINET-E2E-PROBE");

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });
    }
}
