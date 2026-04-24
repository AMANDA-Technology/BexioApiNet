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
using BexioApiNet.Interfaces.Connectors.BusinessActivities;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Interfaces.Connectors.Expenses;
using BexioApiNet.Interfaces.Connectors.Files;
using BexioApiNet.Interfaces.Connectors.Items;
using BexioApiNet.Interfaces.Connectors.MasterData;
using BexioApiNet.Interfaces.Connectors.Payroll;
using BexioApiNet.Interfaces.Connectors.Projects;
using BexioApiNet.Interfaces.Connectors.Purchases;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Interfaces.Connectors.Sales.Positions;
using BexioApiNet.Interfaces.Connectors.Timesheets;
using BexioApiNet.Interfaces.Connectors.Tasks;

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
            Substitute.For<IAdditionalAddressService>(),
            Substitute.For<IInvoiceService>(),
            Substitute.For<IInvoiceReminderService>(),
            Substitute.For<IQuoteService>(),
            Substitute.For<IOrderService>(),
            Substitute.For<IDeliveryService>(),
            Substitute.For<IItemService>(),
            Substitute.For<IUnitService>(),
            Substitute.For<IStockLocationService>(),
            Substitute.For<IStockAreaService>(),
            Substitute.For<IItemPositionService>(),
            Substitute.For<IDefaultPositionService>(),
            Substitute.For<IDiscountPositionService>(),
            Substitute.For<ITextPositionService>(),
            Substitute.For<ISubtotalPositionService>(),
            Substitute.For<ISubPositionService>(),
            Substitute.For<IPagebreakPositionService>(),
            Substitute.For<ITimesheetService>(),
            Substitute.For<ITimesheetStatusService>(),
            Substitute.For<ITaskService>(),
            Substitute.For<ITaskPriorityService>(),
            Substitute.For<ITaskStatusService>(),
            Substitute.For<IBusinessActivityService>(),
            Substitute.For<IProjectService>(),
            Substitute.For<IProjectStateService>(),
            Substitute.For<IProjectTypeService>(),
            Substitute.For<IMilestoneService>(),
            Substitute.For<IPackageService>(),
            Substitute.For<IBillService>(),
            Substitute.For<IPurchaseOrderService>(),
            Substitute.For<IExpenseService>(),
            Substitute.For<IEmployeeService>(),
            Substitute.For<IAbsenceService>(),
            Substitute.For<IPaystubService>(),
            Substitute.For<IFileService>(),
            Substitute.For<IDocumentSettingService>(),
            Substitute.For<IDocumentTemplateService>(),
            Substitute.For<ICountryService>(),
            Substitute.For<ISalutationService>(),
            Substitute.For<ITitleService>(),
            Substitute.For<ILanguageService>(),
            Substitute.For<ICommunicationTypeService>(),
            Substitute.For<ICompanyProfileService>(),
            Substitute.For<IPermissionService>(),
            Substitute.For<IUserService>(),
            Substitute.For<IFictionalUserService>(),
            Substitute.For<INoteService>(),
            Substitute.For<ICommentService>());

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
            Assert.That(client.Invoices, Is.Not.Null);
            Assert.That(client.InvoiceReminders, Is.Not.Null);
            Assert.That(client.Quotes, Is.Not.Null);
            Assert.That(client.Orders, Is.Not.Null);
            Assert.That(client.Deliveries, Is.Not.Null);
            Assert.That(client.Items, Is.Not.Null);
            Assert.That(client.Units, Is.Not.Null);
            Assert.That(client.ItemsStockLocations, Is.Not.Null);
            Assert.That(client.ItemsStockAreas, Is.Not.Null);
            Assert.That(client.SalesItemPositions, Is.Not.Null);
            Assert.That(client.SalesDefaultPositions, Is.Not.Null);
            Assert.That(client.SalesDiscountPositions, Is.Not.Null);
            Assert.That(client.SalesTextPositions, Is.Not.Null);
            Assert.That(client.SalesSubtotalPositions, Is.Not.Null);
            Assert.That(client.SubPositions, Is.Not.Null);
            Assert.That(client.PagebreakPositions, Is.Not.Null);
            Assert.That(client.Timesheets, Is.Not.Null);
            Assert.That(client.TimesheetStatuses, Is.Not.Null);
            Assert.That(client.Tasks, Is.Not.Null);
            Assert.That(client.TaskPriorities, Is.Not.Null);
            Assert.That(client.TaskStatuses, Is.Not.Null);
            Assert.That(client.BusinessActivities, Is.Not.Null);
            Assert.That(client.Projects, Is.Not.Null);
            Assert.That(client.ProjectStates, Is.Not.Null);
            Assert.That(client.ProjectTypes, Is.Not.Null);
            Assert.That(client.Milestones, Is.Not.Null);
            Assert.That(client.Packages, Is.Not.Null);
            Assert.That(client.PurchaseBills, Is.Not.Null);
            Assert.That(client.PurchaseOrders, Is.Not.Null);
            Assert.That(client.Expenses, Is.Not.Null);
            Assert.That(client.PayrollEmployees, Is.Not.Null);
            Assert.That(client.PayrollAbsences, Is.Not.Null);
            Assert.That(client.PayrollPaystubs, Is.Not.Null);
            Assert.That(client.Files, Is.Not.Null);
            Assert.That(client.DocumentSettings, Is.Not.Null);
            Assert.That(client.DocumentTemplates, Is.Not.Null);
            Assert.That(client.Countries, Is.Not.Null);
            Assert.That(client.Salutations, Is.Not.Null);
            Assert.That(client.Titles, Is.Not.Null);
            Assert.That(client.Languages, Is.Not.Null);
            Assert.That(client.CommunicationTypes, Is.Not.Null);
            Assert.That(client.CompanyProfiles, Is.Not.Null);
            Assert.That(client.Permissions, Is.Not.Null);
            Assert.That(client.Users, Is.Not.Null);
            Assert.That(client.FictionalUsers, Is.Not.Null);
            Assert.That(client.Notes, Is.Not.Null);
            Assert.That(client.Comments, Is.Not.Null);
        });
    }
}
