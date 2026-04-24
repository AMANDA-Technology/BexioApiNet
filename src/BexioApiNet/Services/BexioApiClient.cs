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

namespace BexioApiNet.Services;

/// <inheritdoc />
public sealed class BexioApiClient : IBexioApiClient
{
    /// <summary>
    ///     Instance of connection handler used for all services
    /// </summary>
    private readonly IBexioConnectionHandler _bexioConnectionHandler;

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

    /// <inheritdoc />
    public ISubPositionService SubPositions { get; set; }

    /// <inheritdoc />
    public IPagebreakPositionService PagebreakPositions { get; set; }

    /// <inheritdoc />
    public ITimesheetService Timesheets { get; set; }

    /// <inheritdoc />
    public ITimesheetStatusService TimesheetStatuses { get; set; }

    /// <inheritdoc />
    public ITaskService Tasks { get; set; }

    /// <inheritdoc />
    public ITaskPriorityService TaskPriorities { get; set; }

    /// <inheritdoc />
    public ITaskStatusService TaskStatuses { get; set; }

    /// <inheritdoc />
    public IBusinessActivityService BusinessActivities { get; set; }

    /// <inheritdoc />
    public IProjectService Projects { get; set; }

    /// <inheritdoc />
    public IProjectStateService ProjectStates { get; set; }

    /// <inheritdoc />
    public IProjectTypeService ProjectTypes { get; set; }

    /// <inheritdoc />
    public IMilestoneService Milestones { get; set; }

    /// <inheritdoc />
    public IPackageService Packages { get; set; }

    /// <inheritdoc />
    public IBillService PurchaseBills { get; set; }

    /// <inheritdoc />
    public IPurchaseOrderService PurchaseOrders { get; set; }

    /// <inheritdoc />
    public IExpenseService Expenses { get; set; }

    /// <inheritdoc />
    public IEmployeeService PayrollEmployees { get; set; }

    /// <inheritdoc />
    public IAbsenceService PayrollAbsences { get; set; }

    /// <inheritdoc />
    public IPaystubService PayrollPaystubs { get; set; }

    /// <inheritdoc />
    public IFileService Files { get; set; }
    public IDocumentSettingService DocumentSettings { get; set; }

    /// <inheritdoc />
    public IDocumentTemplateService DocumentTemplates { get; set; }
    public ICountryService Countries { get; set; }
    public ISalutationService Salutations { get; set; }
    public ITitleService Titles { get; set; }
    public ILanguageService Languages { get; set; }

    /// <inheritdoc />
    public ICommunicationTypeService CommunicationTypes { get; set; }

    /// <inheritdoc />
    public ICompanyProfileService CompanyProfiles { get; set; }

    /// <inheritdoc />
    public IPermissionService Permissions { get; set; }
    public IUserService Users { get; set; }

    /// <inheritdoc />
    public IFictionalUserService FictionalUsers { get; set; }
    public INoteService Notes { get; set; }

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
        ISubtotalPositionService salesSubtotalPositions,
        ISubPositionService subPositions,
        IPagebreakPositionService pagebreakPositions,
        ITimesheetService timesheets,
        ITimesheetStatusService timesheetStatuses,
        ITaskService tasks,
        ITaskPriorityService taskPriorities,
        ITaskStatusService taskStatuses,
        IBusinessActivityService businessActivities,
        IProjectService projects,
        IProjectStateService projectStates,
        IProjectTypeService projectTypes,
        IMilestoneService milestones,
        IPackageService packages,
        IBillService purchaseBills,
        IPurchaseOrderService purchaseOrders,
        IExpenseService expenses,
        IEmployeeService payrollEmployees,
        IAbsenceService payrollAbsences,
        IPaystubService payrollPaystubs,
        IFileService files)
        IDocumentSettingService documentSettings,
        IDocumentTemplateService documentTemplates)
        ICountryService countries)
        ISalutationService salutations)
        ITitleService titles)
        ILanguageService languages,
        ICommunicationTypeService communicationTypes,
        ICompanyProfileService companyProfiles,
        IPermissionService permissions)
        IUserService users,
        IFictionalUserService fictionalUsers)
        INoteService notes)
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
        SubPositions = subPositions;
        PagebreakPositions = pagebreakPositions;
        Timesheets = timesheets;
        TimesheetStatuses = timesheetStatuses;
        Tasks = tasks;
        TaskPriorities = taskPriorities;
        TaskStatuses = taskStatuses;
        BusinessActivities = businessActivities;
        Projects = projects;
        ProjectStates = projectStates;
        ProjectTypes = projectTypes;
        Milestones = milestones;
        Packages = packages;
        PurchaseBills = purchaseBills;
        PurchaseOrders = purchaseOrders;
        Expenses = expenses;
        PayrollEmployees = payrollEmployees;
        PayrollAbsences = payrollAbsences;
        PayrollPaystubs = payrollPaystubs;
        Files = files;
        DocumentSettings = documentSettings;
        DocumentTemplates = documentTemplates;
        Countries = countries;
        Salutations = salutations;
        Titles = titles;
        Languages = languages;
        CommunicationTypes = communicationTypes;
        CompanyProfiles = companyProfiles;
        Permissions = permissions;
        Users = users;
        FictionalUsers = fictionalUsers;
        Notes = notes;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _bexioConnectionHandler.Dispose();
    }
}