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

using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Enums;
using BexioApiNet.Abstractions.Models.Banking.OutgoingPayments.Views;

namespace BexioApiNet.E2eTests.Tests.Banking.OutgoingPayments;

/// <summary>
/// Live exercises POST → GET → PUT → DELETE on <c>/4.0/purchase/outgoing-payments</c>,
/// asserting the full Create → Read → Update → Delete lifecycle.
/// <para>
/// The test is intentionally left as a stub: the Bexio API enforces many preconditions
/// (existing non-draft bill, sender bank account, business-year window, valid IBAN, etc.)
/// that cannot be asserted in isolation without a fully-seeded test tenant. Setting the
/// env var <c>BexioApiNet__OutgoingPaymentRunMutating</c> to <c>true</c> opts a
/// properly-configured environment into the full create/update/delete cycle.
/// </para>
/// </summary>
public class TestCreateUpdateDelete : OutgoingPaymentE2eTestBase
{
    /// <summary>
    /// End-to-end Create → Read → Update → Delete flow. Skipped by default. Enable by setting
    /// the env vars <c>BexioApiNet__OutgoingPaymentRunMutating=true</c>,
    /// <c>BexioApiNet__OutgoingPaymentBillId</c>, and
    /// <c>BexioApiNet__OutgoingPaymentSenderBankAccountId</c> on a seeded tenant.
    /// </summary>
    [Test]
    public async Task CreateReadUpdateDelete_LifecycleCompletes()
    {
        if (Environment.GetEnvironmentVariable("BexioApiNet__OutgoingPaymentRunMutating") is not { } mutating
            || !bool.TryParse(mutating, out var runMutating)
            || !runMutating)
        {
            Assert.Ignore("mutating E2E disabled — set BexioApiNet__OutgoingPaymentRunMutating=true to run");
            return;
        }

        var billId = RequireBillId();
        Assert.That(OutgoingPayments, Is.Not.Null);

        if (!int.TryParse(Environment.GetEnvironmentVariable("BexioApiNet__OutgoingPaymentSenderBankAccountId"), out var senderBankAccountId))
        {
            Assert.Ignore("BexioApiNet__OutgoingPaymentSenderBankAccountId not configured");
            return;
        }

        var createPayload = new OutgoingPaymentCreate(
            BillId: billId,
            PaymentType: OutgoingPaymentType.MANUAL,
            ExecutionDate: DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Amount: 0.01m,
            CurrencyCode: "CHF",
            ExchangeRate: 1m,
            SenderBankAccountId: senderBankAccountId,
            IsSalaryPayment: false);

        // Create
        var created = await OutgoingPayments!.Create(createPayload);
        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess, Is.True, created.ApiError?.Message);
            Assert.That(created.Data, Is.Not.Null);
            Assert.That(created.Data!.PaymentType, Is.EqualTo(OutgoingPaymentType.MANUAL));
            Assert.That(created.Data.Amount, Is.EqualTo(0.01m));
            Assert.That(created.Data.CurrencyCode, Is.EqualTo("CHF"));
            Assert.That(created.Data.BillId, Is.EqualTo(billId));
        });

        var createdId = created.Data!.Id;

        // Read
        var fetched = await OutgoingPayments.GetById(createdId);
        Assert.Multiple(() =>
        {
            Assert.That(fetched.IsSuccess, Is.True, fetched.ApiError?.Message);
            Assert.That(fetched.Data?.Id, Is.EqualTo(createdId));
        });

        // Update
        var updatePayload = new OutgoingPaymentUpdate(
            PaymentId: createdId,
            ExecutionDate: createPayload.ExecutionDate,
            Amount: createPayload.Amount,
            IsSalaryPayment: false);

        var updated = await OutgoingPayments.Update(updatePayload);
        Assert.Multiple(() =>
        {
            Assert.That(updated.IsSuccess, Is.True, updated.ApiError?.Message);
            Assert.That(updated.Data?.Id, Is.EqualTo(createdId));
        });

        // Delete
        var deleted = await OutgoingPayments.Delete(createdId);
        Assert.That(deleted.IsSuccess, Is.True, deleted.ApiError?.Message);
    }
}
