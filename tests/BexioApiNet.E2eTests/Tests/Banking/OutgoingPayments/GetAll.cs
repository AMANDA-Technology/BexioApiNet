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

namespace BexioApiNet.E2eTests.Tests.Banking.OutgoingPayments;

/// <summary>
/// Live GET <c>/4.0/purchase/outgoing-payments</c> — lists outgoing payments for a bill.
/// Requires <c>BexioApiNet__OutgoingPaymentBillId</c> to be configured.
/// </summary>
public class TestGetAll : OutgoingPaymentE2eTestBase
{
    /// <summary>
    /// Lists outgoing payments filtered by <c>bill_id</c> and asserts the response envelope
    /// deserialises correctly: <c>data</c> list present, <c>paging</c> metadata fields all
    /// populated per the OpenAPI v4.0 schema, and (when populated) every required field
    /// of each <c>OutgoingPaymentListItem</c> is present.
    /// </summary>
    [Test]
    public async Task GetAll()
    {
        var billId = RequireBillId();
        Assert.That(OutgoingPayments, Is.Not.Null);

        var res = await OutgoingPayments!.Get(new QueryParameterOutgoingPayment(billId));

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Data, Is.Not.Null);
            Assert.That(res.Data.Paging, Is.Not.Null);
            Assert.That(res.Data.Paging.Page, Is.GreaterThan(0));
            Assert.That(res.Data.Paging.PageSize, Is.GreaterThan(0));
            Assert.That(res.Data.Paging.PageCount, Is.GreaterThanOrEqualTo(0));
            Assert.That(res.Data.Paging.ItemCount, Is.GreaterThanOrEqualTo(0));
        });

        if (res.Data!.Data.Count > 0)
        {
            var first = res.Data.Data[0];
            Assert.Multiple(() =>
            {
                Assert.That(first.Id, Is.Not.EqualTo(Guid.Empty), "id must be populated per schema");
                Assert.That(first.BillId, Is.Not.EqualTo(Guid.Empty), "bill_id must be populated per schema");
                Assert.That(first.ExecutionDate, Is.Not.EqualTo(default(DateOnly)), "execution_date must be populated per schema");
            });
        }
    }
}
