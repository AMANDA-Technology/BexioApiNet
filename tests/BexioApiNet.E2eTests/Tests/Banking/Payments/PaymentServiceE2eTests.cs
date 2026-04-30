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

using BexioApiNet.Abstractions.Enums.Api;
using BexioApiNet.Abstractions.Models.Banking.Payments;
using BexioApiNet.Abstractions.Models.Banking.Payments.Enums;
using BexioApiNet.Abstractions.Models.Banking.Payments.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Banking;

namespace BexioApiNet.E2eTests.Tests.Banking.Payments;

/// <summary>
/// Live end-to-end smoke tests for <see cref="PaymentService"/>. These tests construct
/// the service directly from a <see cref="BexioConnectionHandler"/> because the
/// aggregate <see cref="IBexioApiClient"/> wire-up lives in a separate issue (#49).
/// All tests skip automatically when <c>BexioApiNet__BaseUri</c> or
/// <c>BexioApiNet__JwtToken</c> is missing.
/// <para>
/// Mutating endpoints (Create, Cancel, Update, Delete) are gated by
/// <c>BexioApiNet__AllowMutatingE2E=true</c> to prevent accidental writes in CI.
/// </para>
/// </summary>
[Category("E2E")]
public sealed class PaymentServiceE2eTests
{
    private BexioConnectionHandler? _connectionHandler;
    private PaymentService? _sut;

    /// <summary>
    /// Reads credentials from environment variables and prepares a
    /// <see cref="PaymentService"/> bound to a live <see cref="BexioConnectionHandler"/>.
    /// Calls <see cref="Assert.Ignore(string)"/> when credentials are absent.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var baseUri = Environment.GetEnvironmentVariable("BexioApiNet__BaseUri");
        var jwtToken = Environment.GetEnvironmentVariable("BexioApiNet__JwtToken");

        if (string.IsNullOrWhiteSpace(baseUri) || string.IsNullOrWhiteSpace(jwtToken))
        {
            Assert.Ignore("credentials not configured");
            return;
        }

        _connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        _sut = new PaymentService(_connectionHandler);
    }

    /// <summary>
    /// Releases the connection handler created for the test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        _connectionHandler?.Dispose();
    }

    /// <summary>
    /// GET <c>4.0/banking/payments</c> returns a successful <see cref="ApiResult{T}"/> and
    /// (when payments exist) every item carries the schema-required <c>uuid</c>,
    /// <c>amount</c>, <c>currency</c>, <c>execution_date</c>, and <c>status</c> fields.
    /// </summary>
    [Test]
    public async Task Get_ReturnsPaymentsMatchingSchemaShape()
    {
        Assert.That(_sut, Is.Not.Null);

        var result = await _sut!.Get();

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
        });

        // Live API may return empty if no payments exist; only assert structure when populated.
        if (result.Data!.Count > 0)
        {
            var first = result.Data[0];
            Assert.Multiple(() =>
            {
                Assert.That(first.Uuid, Is.Not.Null);
                Assert.That(first.Currency, Is.Not.Null);
                Assert.That(first.Amount, Is.Not.Null);
                Assert.That(first.ExecutionDate, Is.Not.Null);
                Assert.That(first.Status, Is.Not.Null);
                Assert.That(first.Type, Is.Not.Null);
            });
        }
    }

    /// <summary>
    /// GET <c>4.0/banking/payments</c> honours pagination parameters (<c>page</c>, <c>per-page</c>).
    /// </summary>
    [Test]
    public async Task Get_WithPagination_ReturnsPayments()
    {
        Assert.That(_sut, Is.Not.Null);

        var result = await _sut!.Get(new QueryParameterPayment(Page: 1, PerPage: 10));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
        });
    }

    /// <summary>
    /// GET <c>4.0/banking/payments/{id}</c> resolves to a payment when the first listed
    /// payment is requested by UUID. Test is skipped when the account has no payments.
    /// </summary>
    [Test]
    public async Task GetById_ReturnsPaymentWithFullSchema()
    {
        Assert.That(_sut, Is.Not.Null);

        var list = await _sut!.Get(new QueryParameterPayment(Page: 1, PerPage: 1));
        if (list.Data is null || list.Data.Count is 0 || list.Data[0].Uuid is null)
        {
            Assert.Ignore("no existing payments to read by id");
            return;
        }

        var result = await _sut.GetById(Guid.Parse(list.Data[0].Uuid!));

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.ApiError, Is.Null);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Uuid, Is.EqualTo(list.Data[0].Uuid));
            Assert.That(result.Data.Currency, Is.Not.Null);
            Assert.That(result.Data.Amount, Is.Not.Null);
            Assert.That(result.Data.ExecutionDate, Is.Not.Null);
            Assert.That(result.Data.Status, Is.Not.Null);
            Assert.That(result.Data.Type, Is.Not.Null);
        });
    }

    /// <summary>
    /// POST <c>4.0/banking/payments</c> creates a payment then verifies the full lifecycle:
    /// the created payment is fetchable by id, can be updated, and can be deleted. Mutating
    /// test — only runs when <c>BexioApiNet__AllowMutatingE2E</c> is set to prevent
    /// accidental writes in CI. <c>BexioApiNet__PaymentAccountId</c> is required.
    /// </summary>
    [Test]
    public async Task CreateUpdateDelete_LifecycleCompletes()
    {
        Assert.That(_sut, Is.Not.Null);

        if (!string.Equals(
                Environment.GetEnvironmentVariable("BexioApiNet__AllowMutatingE2E"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore("set BexioApiNet__AllowMutatingE2E=true to run mutating payment tests");
            return;
        }

        var accountIdValue = Environment.GetEnvironmentVariable("BexioApiNet__PaymentAccountId");
        if (!Guid.TryParse(accountIdValue, out var accountId))
        {
            Assert.Ignore("set BexioApiNet__PaymentAccountId to a valid bank account UUID");
            return;
        }

        var payload = new PaymentCreate(
            Type: PaymentType.iban,
            AccountId: accountId,
            Recipient: new PaymentRecipient(
                Name: "E2E Test Recipient",
                Iban: "CH9300762011623852957",
                Address: new PaymentAddress(
                    StreetName: "Bahnhofstrasse",
                    HouseNumber: "1",
                    Zip: "8001",
                    City: "Zurich",
                    CountryCode: "CH")),
            Amount: 1m,
            Currency: "CHF",
            ExecutionDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            IsSalary: false);

        var created = await _sut!.Create(payload);

        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.IsSuccess, Is.True, created.ApiError?.Message);
            Assert.That(created.Data, Is.Not.Null);
            Assert.That(created.Data!.Uuid, Is.Not.Null);
        });

        var createdId = Guid.Parse(created.Data!.Uuid!);

        var fetched = await _sut.GetById(createdId);
        Assert.Multiple(() =>
        {
            Assert.That(fetched.IsSuccess, Is.True);
            Assert.That(fetched.Data?.Uuid, Is.EqualTo(created.Data.Uuid));
        });

        var updated = await _sut.Update(
            createdId,
            new PaymentUpdate(AdditionalInformation: "Updated by BexioApiNet E2E test"));
        Assert.That(updated.IsSuccess, Is.True, updated.ApiError?.Message);

        var deleted = await _sut.Delete(createdId);
        Assert.That(deleted.IsSuccess, Is.True, deleted.ApiError?.Message);
    }

    /// <summary>
    /// POST <c>4.0/banking/payments/{id}/cancel</c> cancels an existing payment.
    /// Mutating test — gated by <c>BexioApiNet__AllowMutatingE2E</c> and a caller-supplied
    /// payment UUID via <c>BexioApiNet__CancelPaymentId</c>.
    /// </summary>
    [Test]
    public async Task Cancel_CancelsPayment()
    {
        Assert.That(_sut, Is.Not.Null);

        if (!string.Equals(
                Environment.GetEnvironmentVariable("BexioApiNet__AllowMutatingE2E"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.Ignore("set BexioApiNet__AllowMutatingE2E=true to run mutating payment tests");
            return;
        }

        var paymentIdValue = Environment.GetEnvironmentVariable("BexioApiNet__CancelPaymentId");
        if (!Guid.TryParse(paymentIdValue, out var paymentId))
        {
            Assert.Ignore("set BexioApiNet__CancelPaymentId to a cancellable payment UUID");
            return;
        }

        var result = await _sut!.Cancel(paymentId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
    }
}
