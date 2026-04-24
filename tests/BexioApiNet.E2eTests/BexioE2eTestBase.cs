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
using BexioApiNet.Services.Connectors.Accounting;
using BexioApiNet.Services.Connectors.Banking;
using BexioApiNet.Services.Connectors.BusinessActivities;
using BexioApiNet.Services.Connectors.Contacts;
using BexioApiNet.Services.Connectors.Expenses;
using BexioApiNet.Services.Connectors.Items;
using BexioApiNet.Services.Connectors.MasterData;
using BexioApiNet.Services.Connectors.Payroll;
using BexioApiNet.Services.Connectors.Projects;
using BexioApiNet.Services.Connectors.Purchases;
using BexioApiNet.Services.Connectors.Sales;
using BexioApiNet.Services.Connectors.Sales.Positions;
using BexioApiNet.Services.Connectors.Timesheets;
using BexioApiNet.Services.Connectors.Tasks;

namespace BexioApiNet.E2eTests;

/// <summary>
/// Base class for live end-to-end Bexio API tests. Tests inheriting from this class
/// call the real Bexio API and are automatically skipped when credentials are absent.
/// Categorised with <c>[Category("E2E")]</c> so CI runs can filter with
/// <c>dotnet test --filter TestCategory!=E2E</c>.
/// </summary>
[Category("E2E")]
public abstract class BexioE2eTestBase
{
    /// <summary>
    /// Default instance of the Bexio API client. Null when credentials are missing
    /// and the test has been skipped.
    /// </summary>
    protected IBexioApiClient? BexioApiClient;

    /// <summary>
    /// Reads <c>BexioApiNet__BaseUri</c> and <c>BexioApiNet__JwtToken</c> from
    /// environment variables. Calls <see cref="Assert.Ignore(string)"/> if either
    /// is missing so the test suite does not fail CI or AI agent runs that lack
    /// live credentials.
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

        var connectionHandler = new BexioConnectionHandler(
            new BexioConfiguration
            {
                BaseUri = baseUri,
                JwtToken = jwtToken,
                AcceptHeaderFormat = ApiAcceptHeaders.JsonFormatted
            });

        BexioApiClient = new BexioApiClient(
            connectionHandler,
            new BankAccountService(connectionHandler),
            new AccountService(connectionHandler),
            new CurrencyService(connectionHandler),
            new ManualEntryService(connectionHandler),
            new TaxService(connectionHandler),
            new AccountGroupService(connectionHandler),
            new BusinessYearService(connectionHandler),
            new CalendarYearService(connectionHandler),
            new VatPeriodService(connectionHandler),
            new ReportService(connectionHandler),
            new PaymentTypeService(connectionHandler),
            new PaymentService(connectionHandler),
            new OutgoingPaymentService(connectionHandler),
            new ContactService(connectionHandler),
            new ContactGroupService(connectionHandler),
            new ContactRelationService(connectionHandler),
            new ContactSectorService(connectionHandler),
            new AdditionalAddressService(connectionHandler),
            new InvoiceService(connectionHandler),
            new InvoiceReminderService(connectionHandler),
            new QuoteService(connectionHandler),
            new OrderService(connectionHandler),
            new DeliveryService(connectionHandler),
            new ItemService(connectionHandler),
            new UnitService(connectionHandler),
            new StockLocationService(connectionHandler),
            new StockAreaService(connectionHandler),
            new ItemPositionService(connectionHandler),
            new DefaultPositionService(connectionHandler),
            new DiscountPositionService(connectionHandler),
            new TextPositionService(connectionHandler),
            new SubtotalPositionService(connectionHandler),
            new SubPositionService(connectionHandler),
            new PagebreakPositionService(connectionHandler),
            new TimesheetService(connectionHandler),
            new TimesheetStatusService(connectionHandler),
            new TaskService(connectionHandler),
            new TaskPriorityService(connectionHandler),
            new TaskStatusService(connectionHandler),
            new BusinessActivityService(connectionHandler),
            new ProjectService(connectionHandler),
            new ProjectStateService(connectionHandler),
            new ProjectTypeService(connectionHandler),
            new MilestoneService(connectionHandler),
            new PackageService(connectionHandler),
            new BillService(connectionHandler),
            new PurchaseOrderService(connectionHandler),
            new ExpenseService(connectionHandler),
            new EmployeeService(connectionHandler),
            new AbsenceService(connectionHandler),
            new PaystubService(connectionHandler),
            new SalutationService(connectionHandler));
    }

    /// <summary>
    /// Disposes the client if it was created for the test.
    /// </summary>
    [TearDown]
    public void Teardown()
    {
        BexioApiClient?.Dispose();
    }
}
