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

using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Interfaces.Connectors.Contacts;

namespace BexioApiNet.UnitTests;

/// <summary>
/// Wire-up tests for <see cref="BexioApiClient"/>. Verifies that every connector service
/// accepted by the aggregate constructor is exposed via a non-null property so downstream
/// consumers can resolve it through dependency injection.
/// </summary>
[Category("Unit")]
public sealed class BexioApiClientWireUpTests
{
    /// <summary>
    /// Constructing <see cref="BexioApiClient"/> with NSubstitute-provided connectors
    /// must populate every property on the aggregate. A missing assignment in the
    /// constructor or a missing property on <see cref="IBexioApiClient"/> will surface
    /// here as a <c>null</c> property.
    /// </summary>
    [Test]
    public void BexioApiClient_Constructor_AllConnectorPropertiesAreNonNull()
    {
        var connectionHandler = Substitute.For<IBexioConnectionHandler>();

        var client = new BexioApiClient(
            connectionHandler,
            Substitute.For<IBankAccountService>(),
            Substitute.For<IAccountService>(),
            Substitute.For<ICurrencyService>(),
            Substitute.For<IManualEntryService>(),
            Substitute.For<ITaxService>(),
            Substitute.For<IAccountGroupService>(),
            Substitute.For<IBusinessYearService>(),
            Substitute.For<ICalendarYearService>(),
            Substitute.For<IVatPeriodService>(),
            Substitute.For<IReportService>(),
            Substitute.For<IPaymentTypeService>(),
            Substitute.For<IPaymentService>(),
            Substitute.For<IOutgoingPaymentService>(),
            Substitute.For<IContactService>(),
            Substitute.For<IContactGroupService>(),
            Substitute.For<IContactRelationService>(),
            Substitute.For<IContactSectorService>(),
            Substitute.For<IAdditionalAddressService>());

        Assert.Multiple(() =>
        {
            Assert.That(client.BankingBankAccounts, Is.Not.Null);
            Assert.That(client.Accounts, Is.Not.Null);
            Assert.That(client.Currencies, Is.Not.Null);
            Assert.That(client.AccountingManualEntries, Is.Not.Null);
            Assert.That(client.Taxes, Is.Not.Null);
            Assert.That(client.AccountGroups, Is.Not.Null);
            Assert.That(client.AccountingBusinessYears, Is.Not.Null);
            Assert.That(client.AccountingCalendarYears, Is.Not.Null);
            Assert.That(client.AccountingVatPeriods, Is.Not.Null);
            Assert.That(client.AccountingReports, Is.Not.Null);
            Assert.That(client.PaymentTypes, Is.Not.Null);
            Assert.That(client.BankingPayments, Is.Not.Null);
            Assert.That(client.PurchaseOutgoingPayments, Is.Not.Null);
            Assert.That(client.Contacts, Is.Not.Null);
            Assert.That(client.ContactGroups, Is.Not.Null);
            Assert.That(client.ContactRelations, Is.Not.Null);
            Assert.That(client.ContactSectors, Is.Not.Null);
            Assert.That(client.ContactAdditionalAddresses, Is.Not.Null);
        });
    }
}
