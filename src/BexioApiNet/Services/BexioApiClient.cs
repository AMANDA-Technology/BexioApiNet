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

using BexioApiNet.Interfaces;
using BexioApiNet.Interfaces.Connectors.Accounting;
using BexioApiNet.Interfaces.Connectors.Banking;
using BexioApiNet.Interfaces.Connectors.Contacts;
using BexioApiNet.Interfaces.Connectors.Items;
using BexioApiNet.Interfaces.Connectors.Sales;
using BexioApiNet.Interfaces.Connectors.Sales.Positions;

namespace BexioApiNet.Services;

/// <inheritdoc />
public sealed class BexioApiClient : IBexioApiClient
{
    /// <summary>
    ///     Instance of connection handler used for all services
    /// </summary>
    private readonly IBexioConnectionHandler _bexioConnectionHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BexioApiClient" /> class.
    /// </summary>
    public BexioApiClient(
        IBexioConnectionHandler bexioConnectionHandler,
        IBankAccountService bankingBankAccounts,
        IAccountService accountingAccounts,
        ICurrencyService currencies,
        IManualEntryService accountingManualEntries,
        ITaxService taxes,
        IAccountGroupService accountGroups,
        IBusinessYearService accountingBusinessYears,
        ICalendarYearService accountingCalendarYears,
        IVatPeriodService accountingVatPeriods,
        IReportService accountingReports,
        IPaymentTypeService paymentTypes,
        IPaymentService bankingPayments,
        IOutgoingPaymentService purchaseOutgoingPayments,
        IContactService contacts,
        IContactGroupService contactGroups,
        IContactRelationService contactRelations,
        IContactSectorService contactSectors,
        IAdditionalAddressService contactAdditionalAddresses,
        IInvoiceService invoices,
        IInvoiceReminderService invoiceReminders,
        IQuoteService quotes,
        IOrderService orders,
        IDeliveryService deliveries,
        IDiscountPositionService salesDiscountPositions,
        ITextPositionService salesTextPositions,
        ISubtotalPositionService salesSubtotalPositions)
    {
        _bexioConnectionHandler = bexioConnectionHandler;
        BankingBankAccounts = bankingBankAccounts;
        Accounts = accountingAccounts;
        Currencies = currencies;
        AccountingManualEntries = accountingManualEntries;
        Taxes = taxes;
        AccountGroups = accountGroups;
        AccountingBusinessYears = accountingBusinessYears;
        AccountingCalendarYears = accountingCalendarYears;
        AccountingVatPeriods = accountingVatPeriods;
        AccountingReports = accountingReports;
        PaymentTypes = paymentTypes;
        BankingPayments = bankingPayments;
        PurchaseOutgoingPayments = purchaseOutgoingPayments;
        Contacts = contacts;
        ContactGroups = contactGroups;
        ContactRelations = contactRelations;
        ContactSectors = contactSectors;
        ContactAdditionalAddresses = contactAdditionalAddresses;
        Invoices = invoices;
        InvoiceReminders = invoiceReminders;
        Quotes = quotes;
        Orders = orders;
        Deliveries = deliveries;
        SalesDiscountPositions = salesDiscountPositions;
        SalesTextPositions = salesTextPositions;
        SalesSubtotalPositions = salesSubtotalPositions;
    }

    /// <inheritdoc />
    public IBankAccountService BankingBankAccounts { get; set; }

    /// <inheritdoc />
    public IAccountService Accounts { get; set; }

    /// <inheritdoc />
    public ICurrencyService Currencies { get; set; }

    /// <inheritdoc />
    public IManualEntryService AccountingManualEntries { get; set; }

    /// <inheritdoc />
    public ITaxService Taxes { get; set; }

    /// <inheritdoc />
    public IAccountGroupService AccountGroups { get; set; }

    /// <inheritdoc />
    public IBusinessYearService AccountingBusinessYears { get; set; }

    /// <inheritdoc />
    public ICalendarYearService AccountingCalendarYears { get; set; }

    /// <inheritdoc />
    public IVatPeriodService AccountingVatPeriods { get; set; }

    /// <inheritdoc />
    public IReportService AccountingReports { get; set; }

    /// <inheritdoc />
    public IPaymentTypeService PaymentTypes { get; set; }

    /// <inheritdoc />
    public IPaymentService BankingPayments { get; set; }

    /// <inheritdoc />
    public IOutgoingPaymentService PurchaseOutgoingPayments { get; set; }

    /// <inheritdoc />
    public IContactService Contacts { get; set; }

    /// <inheritdoc />
    public IContactGroupService ContactGroups { get; set; }

    /// <inheritdoc />
    public IContactRelationService ContactRelations { get; set; }

    /// <inheritdoc />
    public IContactSectorService ContactSectors { get; set; }

    /// <inheritdoc />
    public IAdditionalAddressService ContactAdditionalAddresses { get; set; }

    /// <inheritdoc />
    public IInvoiceService Invoices { get; set; }

    /// <inheritdoc />
    public IInvoiceReminderService InvoiceReminders { get; set; }

    /// <inheritdoc />
    public IQuoteService Quotes { get; set; }

    /// <inheritdoc />
    public IOrderService Orders { get; set; }

    /// <inheritdoc />
    public IDeliveryService Deliveries { get; set; }

    /// <inheritdoc />
    public IItemService Items { get; set; }

    /// <inheritdoc />
    public IUnitService Units { get; set; }

    /// <inheritdoc />
    public IStockLocationService ItemsStockLocations { get; set; }

    /// <inheritdoc />
    public IStockAreaService ItemsStockAreas { get; set; }

    /// <inheritdoc />
    public IItemPositionService SalesItemPositions { get; set; }

    /// <inheritdoc />
    public IDefaultPositionService SalesDefaultPositions { get; set; }

    /// <inheritdoc />
    public IDiscountPositionService SalesDiscountPositions { get; set; }

    /// <inheritdoc />
    public ITextPositionService SalesTextPositions { get; set; }

    /// <inheritdoc />
    public ISubtotalPositionService SalesSubtotalPositions { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BexioApiClient"/> class.
    /// </summary>
    public BexioApiClient(
        IBexioConnectionHandler bexioConnectionHandler,
        IBankAccountService bankingBankAccounts,
        IAccountService accountingAccounts,
        ICurrencyService currencies,
        IManualEntryService accountingManualEntries,
        ITaxService taxes,
        IAccountGroupService accountGroups,
        IBusinessYearService accountingBusinessYears,
        ICalendarYearService accountingCalendarYears,
        IVatPeriodService accountingVatPeriods,
        IReportService accountingReports,
        IPaymentTypeService paymentTypes,
        IPaymentService bankingPayments,
        IOutgoingPaymentService purchaseOutgoingPayments,
        IContactService contacts,
        IContactGroupService contactGroups,
        IContactRelationService contactRelations,
        IContactSectorService contactSectors,
        IAdditionalAddressService contactAdditionalAddresses,
        IInvoiceService invoices,
        IInvoiceReminderService invoiceReminders,
        IQuoteService quotes,
        IOrderService orders,
        IDeliveryService deliveries,
        IItemService items,
        IUnitService units,
        IStockLocationService itemsStockLocations,
        IStockAreaService itemsStockAreas,
        IItemPositionService salesItemPositions,
        IDefaultPositionService salesDefaultPositions,
        IDiscountPositionService salesDiscountPositions,
        ITextPositionService salesTextPositions,
        ISubtotalPositionService salesSubtotalPositions)
    {
        _bexioConnectionHandler = bexioConnectionHandler;
        BankingBankAccounts = bankingBankAccounts;
        Accounts = accountingAccounts;
        Currencies = currencies;
        AccountingManualEntries = accountingManualEntries;
        Taxes = taxes;
        AccountGroups = accountGroups;
        AccountingBusinessYears = accountingBusinessYears;
        AccountingCalendarYears = accountingCalendarYears;
        AccountingVatPeriods = accountingVatPeriods;
        AccountingReports = accountingReports;
        PaymentTypes = paymentTypes;
        BankingPayments = bankingPayments;
        PurchaseOutgoingPayments = purchaseOutgoingPayments;
        Contacts = contacts;
        ContactGroups = contactGroups;
        ContactRelations = contactRelations;
        ContactSectors = contactSectors;
        ContactAdditionalAddresses = contactAdditionalAddresses;
        Invoices = invoices;
        InvoiceReminders = invoiceReminders;
        Quotes = quotes;
        Orders = orders;
        Deliveries = deliveries;
        Items = items;
        Units = units;
        ItemsStockLocations = itemsStockLocations;
        ItemsStockAreas = itemsStockAreas;
        SalesItemPositions = salesItemPositions;
        SalesDefaultPositions = salesDefaultPositions;
        SalesDiscountPositions = salesDiscountPositions;
        SalesTextPositions = salesTextPositions;
        SalesSubtotalPositions = salesSubtotalPositions;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _bexioConnectionHandler.Dispose();
    }
}