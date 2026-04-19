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
/// Live GET <c>/4.0/purchase/outgoing-payments/{id}</c> — fetches a single outgoing payment
/// by id. The test first lists payments for the configured bill to discover a valid id; if
/// the list is empty the test is skipped.
/// </summary>
public class TestGetById : OutgoingPaymentE2eTestBase
{
    /// <summary>
    /// Lists existing payments to discover an id, then fetches that payment by id and asserts
    /// the full model deserialises with a matching <c>Id</c>.
    /// </summary>
    [Test]
    public async Task GetById()
    {
        var billId = RequireBillId();
        Assert.That(OutgoingPayments, Is.Not.Null);

        var list = await OutgoingPayments!.Get(new QueryParameterOutgoingPayment(billId));
        Assert.That(list.IsSuccess, Is.True);

        if (list.Data is null || list.Data.Data.Count == 0)
        {
            Assert.Ignore("no existing outgoing payments available for bill");
            return;
        }

        var firstId = list.Data.Data[0].Id;

        var res = await OutgoingPayments.GetById(firstId);

        Assert.That(res, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(res.IsSuccess, Is.True);
            Assert.That(res.ApiError, Is.Null);
            Assert.That(res.Data, Is.Not.Null);
            Assert.That(res.Data!.Id, Is.EqualTo(firstId));
        });
    }
}
