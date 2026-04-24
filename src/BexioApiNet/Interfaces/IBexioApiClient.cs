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
using BexioApiNet.Interfaces.Connectors.Items;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Interfaces.Connectors.Sales.Positions;
using BexioApiNet.Interfaces.Connectors.Timesheets;
using BexioApiNet.Interfaces.Connectors.Tasks;

namespace BexioApiNet.Interfaces;

/// <summary>
///     Connector service to call bexio REST API. <see href="https://docs.bexio.com/">bexio API (3.0.0)</see>
/// </summary>
public interface IBexioApiClient : IDisposable
{
    /// <summary>
    ///     Bexio bank account connector. <see href="https://docs.bexio.com/#tag/Bank-Accounts">Bank-Accounts</see>
    /// </summary>
    public IBankAccountService BankingBankAccounts { get; set; }

    /// <summary>
    ///     Bexio account connector. <see href="https://docs.bexio.com/#tag/Accounts">Accounts</see>
    /// </summary>
    public IAccountService Accounts { get; set; }

    /// <summary>
    ///     Bexio currency connector. <see href="https://docs.bexio.com/#tag/Currencies">Currencies</see>
    /// </summary>
    public ICurrencyService Currencies { get; set; }

    /// <summary>
    ///     Bexio account manual entry connector. <see href="https://docs.bexio.com/#tag/Manual-Entries">Manual Entries</see>
    /// </summary>
    public IManualEntryService AccountingManualEntries { get; set; }

    /// <summary>
    ///     Bexio currency connector. <see href="https://docs.bexio.com/#tag/Taxes">Taxes</see>
    /// </summary>
    public ITaxService Taxes { get; set; }

    /// <summary>
    ///     Bexio account group connector. <see href="https://docs.bexio.com/#tag/Account-Groups">Account Groups</see>
    /// </summary>
    public IAccountGroupService AccountGroups { get; set; }

    /// <summary>
    ///     Bexio accounting business years connector.
    ///     <see href="https://docs.bexio.com/#tag/Business-Years">Business Years</see>
    /// </summary>
    public IBusinessYearService AccountingBusinessYears { get; set; }

    /// <summary>
    ///     Bexio accounting calendar years connector.
    ///     <see href="https://docs.bexio.com/#tag/Calendar-Years">Calendar Years</see>
    /// </summary>
    public ICalendarYearService AccountingCalendarYears { get; set; }

    /// <summary>
    ///     Bexio accounting VAT periods connector. <see href="https://docs.bexio.com/#tag/Vat-Periods">Vat Periods</see>
    /// </summary>
    public IVatPeriodService AccountingVatPeriods { get; set; }

    /// <summary>
    ///     Bexio accounting reports connector. <see href="https://docs.bexio.com/#tag/Reports">Reports</see>
    /// </summary>
    public IReportService AccountingReports { get; set; }

    /// <summary>
    ///     Bexio payment types connector. <see href="https://docs.bexio.com/#tag/Payment-Types">Payment Types</see>
    /// </summary>
    public IPaymentTypeService PaymentTypes { get; set; }

    /// <summary>
    ///     Bexio banking payments connector. <see href="https://docs.bexio.com/#tag/Payments">Payments</see>
    /// </summary>
    public IPaymentService BankingPayments { get; set; }

    /// <summary>
    ///     Bexio purchase outgoing payments connector.
    ///     <see href="https://docs.bexio.com/#tag/Outgoing-Payment">Outgoing Payment</see>
    /// </summary>
    public IOutgoingPaymentService PurchaseOutgoingPayments { get; set; }

    /// <summary>
    ///     Bexio contacts connector. <see href="https://docs.bexio.com/#tag/Contacts">Contacts</see>
    /// </summary>
    public IContactService Contacts { get; set; }

    /// <summary>
    ///     Bexio contact groups connector. <see href="https://docs.bexio.com/#tag/Contact-Groups">Contact Groups</see>
    /// </summary>
    public IContactGroupService ContactGroups { get; set; }

    /// <summary>
    ///     Bexio contact relations connector.
    ///     <see href="https://docs.bexio.com/#tag/Contact-Relations">Contact Relations</see>
    /// </summary>
    public IContactRelationService ContactRelations { get; set; }

    /// <summary>
    ///     Bexio contact sectors connector. <see href="https://docs.bexio.com/#tag/Contact-Sectors">Contact Sectors</see>
    /// </summary>
    public IContactSectorService ContactSectors { get; set; }

    /// <summary>
    ///     Bexio additional addresses connector, nested under contacts.
    ///     <see href="https://docs.bexio.com/#tag/Additional-Addresses">Additional Addresses</see>
    /// </summary>
    public IAdditionalAddressService ContactAdditionalAddresses { get; set; }

    /// <summary>
    ///     Bexio invoices connector. <see href="https://docs.bexio.com/#tag/Invoices">Invoices</see>
    /// </summary>
    public IInvoiceService Invoices { get; set; }

    /// <summary>
    ///     Bexio invoice reminders connector, nested under invoices.
    ///     <see href="https://docs.bexio.com/#tag/Invoices">Invoices</see>
    /// </summary>
    public IInvoiceReminderService InvoiceReminders { get; set; }

    /// <summary>
    ///     Bexio quotes (offers) connector. <see href="https://docs.bexio.com/#tag/Quotes">Quotes</see>
    /// </summary>
    public IQuoteService Quotes { get; set; }

    /// <summary>
    ///     Bexio orders (customer confirmations) connector. <see href="https://docs.bexio.com/#tag/Orders">Orders</see>
    /// </summary>
    public IOrderService Orders { get; set; }

    /// <summary>
    ///     Bexio deliveries connector. <see href="https://docs.bexio.com/#tag/Deliveries">Deliveries</see>
    /// </summary>
    public IDeliveryService Deliveries { get; set; }

    /// <summary>
    /// Bexio items connector. <see href="https://docs.bexio.com/#tag/Items">Items</see>
    /// </summary>
    public IItemService Items { get; set; }

    /// <summary>
    /// Bexio units connector. <see href="https://docs.bexio.com/#tag/Units">Units</see>
    /// </summary>
    public IUnitService Units { get; set; }

    /// <summary>
    /// Bexio stock locations connector. <see href="https://docs.bexio.com/#tag/Stock-locations">Stock locations</see>
    /// </summary>
    public IStockLocationService ItemsStockLocations { get; set; }

    /// <summary>
    /// Bexio stock areas connector. <see href="https://docs.bexio.com/#tag/Stock-Areas">Stock Areas</see>
    /// </summary>
    public IStockAreaService ItemsStockAreas { get; set; }

    /// <summary>
    /// Bexio article (item) position connector, shared across invoice/quote/order documents.
    /// <see href="https://docs.bexio.com/#tag/Article-positions">Article positions</see>
    /// </summary>
    public IItemPositionService SalesItemPositions { get; set; }

    /// <summary>
    /// Bexio custom (default) position connector, shared across invoice/quote/order documents.
    /// <see href="https://docs.bexio.com/#tag/Custom-positions">Custom positions</see>
    /// </summary>
    public IDefaultPositionService SalesDefaultPositions { get; set; }

    /// <summary>
    ///     Bexio discount positions connector, nested under sales documents.
    ///     <see href="https://docs.bexio.com/#tag/Discount-positions">Discount Positions</see>
    /// </summary>
    public IDiscountPositionService SalesDiscountPositions { get; set; }

    /// <summary>
    ///     Bexio text positions connector, nested under sales documents.
    ///     <see href="https://docs.bexio.com/#tag/Text-positions">Text Positions</see>
    /// </summary>
    public ITextPositionService SalesTextPositions { get; set; }

    /// <summary>
    ///     Bexio subtotal positions connector, nested under sales documents.
    ///     <see href="https://docs.bexio.com/#tag/Subtotal-positions">Subtotal Positions</see>
    /// </summary>
    public ISubtotalPositionService SalesSubtotalPositions { get; set; }

    /// <summary>
    /// Bexio document sub-positions connector. <see href="https://docs.bexio.com/#tag/Sub-positions">Sub-positions</see>
    /// </summary>
    public ISubPositionService SubPositions { get; set; }

    /// <summary>
    /// Bexio document pagebreak-positions connector. <see href="https://docs.bexio.com/#tag/Pagebreak-positions">Pagebreak-positions</see>
    /// </summary>
    public IPagebreakPositionService PagebreakPositions { get; set; }

    /// <summary>
    /// Bexio timesheets connector. <see href="https://docs.bexio.com/#tag/Timesheets">Timesheets</see>
    /// </summary>
    public ITimesheetService Timesheets { get; set; }

    /// <summary>
    /// Bexio timesheet status lookup connector. <see href="https://docs.bexio.com/#tag/Timesheets/operation/v2ListTimeSheetStatus">Timesheet Status</see>
    /// </summary>
    public ITimesheetStatusService TimesheetStatuses { get; set; }

    /// <summary>
    /// Bexio tasks connector. <see href="https://docs.bexio.com/#tag/Tasks">Tasks</see>
    /// </summary>
    public ITaskService Tasks { get; set; }

    /// <summary>
    /// Bexio task priorities connector (read-only lookup). <see href="https://docs.bexio.com/#tag/Tasks/operation/v2ListTaskPriority">List Task Priority</see>
    /// </summary>
    public ITaskPriorityService TaskPriorities { get; set; }

    /// <summary>
    /// Bexio task statuses connector (read-only lookup). <see href="https://docs.bexio.com/#tag/Tasks/operation/v2ListTaskStatus">List Task Status</see>
    /// </summary>
    public ITaskStatusService TaskStatuses { get; set; }
}
