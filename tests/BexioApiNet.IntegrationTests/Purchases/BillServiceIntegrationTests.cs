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

using BexioApiNet.Abstractions.Models.Purchases.Bills;
using BexioApiNet.Abstractions.Models.Purchases.Bills.Enums;
using BexioApiNet.Abstractions.Models.Purchases.Bills.Views;
using BexioApiNet.Models;
using BexioApiNet.Services.Connectors.Purchases;

namespace BexioApiNet.IntegrationTests.Purchases;

/// <summary>
/// Integration tests for <see cref="BillService"/> against WireMock stubs. Verifies the
/// path composed from <see cref="BillConfiguration"/> (<c>4.0/purchase/bills</c> and
/// <c>4.0/purchase/documentnumbers/bills</c>) reaches the handler correctly, that the
/// expected HTTP verbs are used for each operation — including <c>PUT</c> for Update and
/// UpdateBookings per the v3.0.0 OpenAPI spec — and that responses round-trip through the
/// canonical Bill, BillListResponse and BillDocumentNumberResponse schemas.
/// </summary>
public sealed class BillServiceIntegrationTests : IntegrationTestBase
{
    private const string BillsPath = "/4.0/purchase/bills";
    private const string DocNumbersPath = "/4.0/purchase/documentnumbers/bills";

    private static readonly Guid TestBillId = Guid.Parse("64bf865d-988a-496d-a24f-bab2d52e4b4a");

    /// <summary>
    /// Fully-populated Bill response matching every field of the v3.0.0 OpenAPI <c>Bill</c>
    /// schema. Used to exercise full deserialization through <see cref="Bill"/>.
    /// </summary>
    private const string FullBillResponse = """
                                            {
                                                "id": "64bf865d-988a-496d-a24f-bab2d52e4b4a",
                                                "document_no": "LR-12345",
                                                "title": "Bill 42",
                                                "status": "DRAFT",
                                                "firstname_suffix": "LeSS",
                                                "lastname_company": "Organisation",
                                                "created_at": "2026-02-12T09:53:49",
                                                "supplier_id": 1323,
                                                "vendor_ref": "Reference text",
                                                "pending_amount": 65.23,
                                                "amount_man": 23.87,
                                                "amount_calc": 23.90,
                                                "manual_amount": true,
                                                "contact_partner_id": 647,
                                                "bill_date": "2026-02-12",
                                                "due_date": "2026-03-14",
                                                "currency_code": "USD",
                                                "exchange_rate": 2.3455365492,
                                                "base_currency_code": "USD",
                                                "item_net": false,
                                                "split_into_line_items": true,
                                                "purchase_order_id": 637,
                                                "base_currency_amount": 75.23,
                                                "overdue": true,
                                                "qr_bill_information": "//S1/10/10201409/11/190512/20/1400.000-53/30/106017086/31/180508/32/7.7/40/2:10;0:30",
                                                "address": {
                                                    "title": "Prof",
                                                    "salutation": "Mrs",
                                                    "firstname_suffix": "John",
                                                    "lastname_company": "Newman",
                                                    "address_line": "Mega Street",
                                                    "postcode": "6694",
                                                    "city": "Tel Aviv",
                                                    "country_code": "CH",
                                                    "main_contact_id": 45,
                                                    "contact_address_id": 827,
                                                    "type": "PRIVATE"
                                                },
                                                "line_items": [
                                                    {
                                                        "id": "2d267f64-6b94-4109-818e-c54515837004",
                                                        "position": 0,
                                                        "title": "First line item title",
                                                        "tax_id": 15,
                                                        "tax_calc": 12.89,
                                                        "amount": 56.80,
                                                        "booking_account_id": 16
                                                    }
                                                ],
                                                "discounts": [
                                                    {
                                                        "id": "8b102a32-5bef-462e-a41b-9c00197c26b9",
                                                        "position": 1,
                                                        "amount": 56.80
                                                    }
                                                ],
                                                "payment": {
                                                    "type": "IBAN",
                                                    "bank_account_id": 12,
                                                    "fee": "BY_SENDER",
                                                    "execution_date": "2026-03-15",
                                                    "exchange_rate": 2.34553,
                                                    "amount": 3.90,
                                                    "iban": "CH121234567812345678900",
                                                    "name": "LeSS Organisation",
                                                    "address": "1147 Super Street",
                                                    "street": "Super Street",
                                                    "house_no": "1147",
                                                    "postcode": "9999",
                                                    "city": "Tel Aviv",
                                                    "country_code": "CH",
                                                    "message": "This is a message.",
                                                    "booking_text": "Further education.",
                                                    "salary_payment": false,
                                                    "reference_no": "1212345675321984798456",
                                                    "note": "Some note text"
                                                },
                                                "attachment_ids": [
                                                    "e84b9fe2-3fe2-4fcf-8c30-298fe16adb14",
                                                    "aa9fc418-f292-49ad-9a35-9869123d1091"
                                                ]
                                            }
                                            """;

    /// <summary>
    /// Fully-populated Bills list response matching every field of the v3.0.0 OpenAPI
    /// list-envelope schema. Includes two list items with different statuses to exercise
    /// the <see cref="BillStatus"/> enum and the paging metadata.
    /// </summary>
    private const string FullBillListResponse = """
                                                {
                                                    "data": [
                                                        {
                                                            "id": "2af7df09-bf6b-4a6b-840f-142e337e692a",
                                                            "created_at": "2026-03-23T09:53:49+00:00",
                                                            "document_no": "NO-1",
                                                            "status": "DRAFT",
                                                            "vendor_ref": "Vendor 1",
                                                            "firstname_suffix": "John",
                                                            "lastname_company": "Doe",
                                                            "vendor": "John Doe",
                                                            "title": "Title 1",
                                                            "currency_code": "CHF",
                                                            "pending_amount": 100.23,
                                                            "net": 0.45,
                                                            "gross": 13.42,
                                                            "bill_date": "2026-02-12",
                                                            "due_date": "2026-03-14",
                                                            "overdue": false,
                                                            "booking_account_ids": [10, 12],
                                                            "attachment_ids": [
                                                                "1cb712f3-652c-4707-9641-2de94f77e07d",
                                                                "ab2b0d50-f3b0-4773-9c65-6606657db25b",
                                                                "34ef8407-094a-419f-b649-789d36b5d145"
                                                            ]
                                                        },
                                                        {
                                                            "id": "99fd6dc2-09cf-4db6-8dfa-2b9b3b9394b1",
                                                            "created_at": "2026-05-23T09:53:49+00:00",
                                                            "document_no": "NO-3",
                                                            "status": "BOOKED",
                                                            "vendor_ref": "Vendor 2",
                                                            "firstname_suffix": "James",
                                                            "lastname_company": "Doe",
                                                            "vendor": "James Doe",
                                                            "title": "Title 2",
                                                            "currency_code": "USD",
                                                            "pending_amount": 2.73,
                                                            "net": 0.01,
                                                            "gross": 1.42,
                                                            "bill_date": "2026-04-02",
                                                            "due_date": "2026-05-27",
                                                            "overdue": true,
                                                            "booking_account_ids": [12, 134, 9],
                                                            "attachment_ids": [
                                                                "1f1ef73d-6b4a-4de5-812c-27f8732be88b",
                                                                "d9d3a328-8c0b-4889-9b15-d3e9abc24df0"
                                                            ]
                                                        }
                                                    ],
                                                    "paging": {
                                                        "page": 1,
                                                        "page_size": 10,
                                                        "page_count": 50,
                                                        "item_count": 300
                                                    }
                                                }
                                                """;

    private const string DocumentNumberBody = """
                                              {
                                                  "valid": false,
                                                  "next_available_no": "AB-1235"
                                              }
                                              """;

    /// <summary>
    /// <c>BillService.Get</c> issues a <c>GET</c> against <c>/4.0/purchase/bills</c>
    /// and deserializes the full paged envelope response — including the <c>data</c>
    /// array with every list-item field and the <c>paging</c> block.
    /// </summary>
    [Test]
    public async Task BillService_Get_SendsGetRequest_DeserializesFullList()
    {
        Server
            .Given(Request.Create().WithPath(BillsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FullBillListResponse));

        var service = new BillService(ConnectionHandler);

        var result = await service.Get(cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BillsPath));
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Paging.Page, Is.EqualTo(1));
            Assert.That(result.Data.Paging.PageSize, Is.EqualTo(10));
            Assert.That(result.Data.Paging.PageCount, Is.EqualTo(50));
            Assert.That(result.Data.Paging.ItemCount, Is.EqualTo(300));
            Assert.That(result.Data.Data, Has.Count.EqualTo(2));

            var first = result.Data.Data[0];
            Assert.That(first.Id, Is.EqualTo(Guid.Parse("2af7df09-bf6b-4a6b-840f-142e337e692a")));
            Assert.That(first.DocumentNo, Is.EqualTo("NO-1"));
            Assert.That(first.Status, Is.EqualTo(BillStatus.DRAFT));
            Assert.That(first.Vendor, Is.EqualTo("John Doe"));
            Assert.That(first.LastnameCompany, Is.EqualTo("Doe"));
            Assert.That(first.FirstnameSuffix, Is.EqualTo("John"));
            Assert.That(first.CurrencyCode, Is.EqualTo("CHF"));
            Assert.That(first.PendingAmount, Is.EqualTo(100.23m));
            Assert.That(first.Net, Is.EqualTo(0.45m));
            Assert.That(first.Gross, Is.EqualTo(13.42m));
            Assert.That(first.BillDate, Is.EqualTo(new DateOnly(2026, 2, 12)));
            Assert.That(first.DueDate, Is.EqualTo(new DateOnly(2026, 3, 14)));
            Assert.That(first.Overdue, Is.False);
            Assert.That(first.BookingAccountIds, Is.EquivalentTo(new[] { 10, 12 }));
            Assert.That(first.AttachmentIds, Has.Count.EqualTo(3));

            var second = result.Data.Data[1];
            Assert.That(second.Status, Is.EqualTo(BillStatus.BOOKED));
            Assert.That(second.Overdue, Is.True);
        });
    }

    /// <summary>
    /// <c>BillService.Get</c> with a populated <see cref="QueryParameterBill" /> appends
    /// every spec-supported parameter to the URL.
    /// </summary>
    [Test]
    public async Task BillService_Get_WithQueryParameter_AppendsAllSupportedKeys()
    {
        Server
            .Given(Request.Create().WithPath(BillsPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FullBillListResponse));

        var service = new BillService(ConnectionHandler);

        var result = await service.Get(
            new QueryParameterBill(
                limit: 10,
                page: 2,
                order: "asc",
                sort: "document_no",
                status: "TODO",
                billDateStart: new DateOnly(2026, 1, 1),
                supplierId: 1323),
            cancellationToken: TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Url, Does.Contain("limit=10"));
            Assert.That(request.Url, Does.Contain("page=2"));
            Assert.That(request.Url, Does.Contain("order=asc"));
            Assert.That(request.Url, Does.Contain("sort=document_no"));
            Assert.That(request.Url, Does.Contain("status=TODO"));
            Assert.That(request.Url, Does.Contain("bill_date_start=2026-01-01"));
            Assert.That(request.Url, Does.Contain("supplier_id=1323"));
        });
    }

    /// <summary>
    /// <c>BillService.GetById</c> issues a <c>GET</c> request with the bill id
    /// in the URL path and deserializes every property of the full Bill schema —
    /// scalar fields, the nested address, line items, discounts, the payment block
    /// and the attachment ids.
    /// </summary>
    [Test]
    public async Task BillService_GetById_DeserializesFullBill()
    {
        var expectedPath = $"{BillsPath}/{TestBillId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FullBillResponse));

        var service = new BillService(ConnectionHandler);

        var result = await service.GetById(TestBillId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(result.Data, Is.Not.Null);

            var bill = result.Data!;
            Assert.That(bill.Id, Is.EqualTo(TestBillId));
            Assert.That(bill.DocumentNo, Is.EqualTo("LR-12345"));
            Assert.That(bill.Title, Is.EqualTo("Bill 42"));
            Assert.That(bill.Status, Is.EqualTo(BillStatus.DRAFT));
            Assert.That(bill.FirstnameSuffix, Is.EqualTo("LeSS"));
            Assert.That(bill.LastnameCompany, Is.EqualTo("Organisation"));
            Assert.That(bill.SupplierId, Is.EqualTo(1323));
            Assert.That(bill.VendorRef, Is.EqualTo("Reference text"));
            Assert.That(bill.PendingAmount, Is.EqualTo(65.23m));
            Assert.That(bill.AmountMan, Is.EqualTo(23.87m));
            Assert.That(bill.AmountCalc, Is.EqualTo(23.90m));
            Assert.That(bill.ManualAmount, Is.True);
            Assert.That(bill.ContactPartnerId, Is.EqualTo(647));
            Assert.That(bill.BillDate, Is.EqualTo(new DateOnly(2026, 2, 12)));
            Assert.That(bill.DueDate, Is.EqualTo(new DateOnly(2026, 3, 14)));
            Assert.That(bill.CurrencyCode, Is.EqualTo("USD"));
            Assert.That(bill.ExchangeRate, Is.EqualTo(2.3455365492m));
            Assert.That(bill.BaseCurrencyCode, Is.EqualTo("USD"));
            Assert.That(bill.ItemNet, Is.False);
            Assert.That(bill.SplitIntoLineItems, Is.True);
            Assert.That(bill.PurchaseOrderId, Is.EqualTo(637));
            Assert.That(bill.BaseCurrencyAmount, Is.EqualTo(75.23m));
            Assert.That(bill.Overdue, Is.True);
            Assert.That(bill.QrBillInformation, Does.StartWith("//S1/"));

            Assert.That(bill.Address.LastnameCompany, Is.EqualTo("Newman"));
            Assert.That(bill.Address.Type, Is.EqualTo(BillAddressType.PRIVATE));
            Assert.That(bill.Address.Title, Is.EqualTo("Prof"));
            Assert.That(bill.Address.Salutation, Is.EqualTo("Mrs"));
            Assert.That(bill.Address.City, Is.EqualTo("Tel Aviv"));
            Assert.That(bill.Address.MainContactId, Is.EqualTo(45));

            Assert.That(bill.LineItems, Has.Count.EqualTo(1));
            Assert.That(bill.LineItems[0].Id, Is.EqualTo(Guid.Parse("2d267f64-6b94-4109-818e-c54515837004")));
            Assert.That(bill.LineItems[0].Position, Is.EqualTo(0));
            Assert.That(bill.LineItems[0].Title, Is.EqualTo("First line item title"));
            Assert.That(bill.LineItems[0].TaxId, Is.EqualTo(15));
            Assert.That(bill.LineItems[0].TaxCalc, Is.EqualTo(12.89m));
            Assert.That(bill.LineItems[0].Amount, Is.EqualTo(56.80m));
            Assert.That(bill.LineItems[0].BookingAccountId, Is.EqualTo(16));

            Assert.That(bill.Discounts, Has.Count.EqualTo(1));
            Assert.That(bill.Discounts[0].Id, Is.EqualTo(Guid.Parse("8b102a32-5bef-462e-a41b-9c00197c26b9")));
            Assert.That(bill.Discounts[0].Position, Is.EqualTo(1));
            Assert.That(bill.Discounts[0].Amount, Is.EqualTo(56.80m));

            Assert.That(bill.Payment, Is.Not.Null);
            Assert.That(bill.Payment!.Type, Is.EqualTo(BillPaymentType.IBAN));
            Assert.That(bill.Payment.Fee, Is.EqualTo(BillPaymentFeeType.BY_SENDER));
            Assert.That(bill.Payment.ExecutionDate, Is.EqualTo(new DateOnly(2026, 3, 15)));
            Assert.That(bill.Payment.ExchangeRate, Is.EqualTo(2.34553m));
            Assert.That(bill.Payment.Amount, Is.EqualTo(3.90m));
            Assert.That(bill.Payment.Iban, Is.EqualTo("CH121234567812345678900"));
            Assert.That(bill.Payment.SalaryPayment, Is.False);

            Assert.That(bill.AttachmentIds, Has.Count.EqualTo(2));
            Assert.That(bill.AttachmentIds[0],
                Is.EqualTo(Guid.Parse("e84b9fe2-3fe2-4fcf-8c30-298fe16adb14")));
        });
    }

    /// <summary>
    /// <c>BillService.GetDocNumbers</c> routes to
    /// <c>/4.0/purchase/documentnumbers/bills</c>, passes the proposed number via
    /// the <c>document_no</c> query parameter, and deserializes the response body
    /// into <c>BillDocumentNumberResponse</c> with both fields populated.
    /// </summary>
    [Test]
    public async Task BillService_GetDocNumbers_SendsGetRequestToDocNumberEndpoint()
    {
        Server
            .Given(Request.Create().WithPath(DocNumbersPath).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(DocumentNumberBody));

        var service = new BillService(ConnectionHandler);

        var result = await service.GetDocNumbers("AB-1234", TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Valid, Is.False);
            Assert.That(result.Data.NextAvailableNo, Is.EqualTo("AB-1235"));
            Assert.That(request.Method, Is.EqualTo("GET"));
            Assert.That(request.AbsolutePath, Is.EqualTo(DocNumbersPath));
            Assert.That(request.Url, Does.Contain("document_no=AB-1234"));
        });
    }

    /// <summary>
    /// <c>BillService.Create</c> sends a <c>POST</c> request whose body contains
    /// the serialized <see cref="BillCreate"/> payload (snake_case keys), and the
    /// 201 response deserializes into the canonical <see cref="Bill"/> record.
    /// </summary>
    [Test]
    public async Task BillService_Create_SendsPostRequestWithPayload()
    {
        Server
            .Given(Request.Create().WithPath(BillsPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(201).WithBody(FullBillResponse));

        var service = new BillService(ConnectionHandler);

        var payload = new BillCreate(
            SupplierId: 1323,
            ContactPartnerId: 647,
            CurrencyCode: "USD",
            Address: new BillAddress("Newman", BillAddressType.PRIVATE),
            BillDate: new DateOnly(2026, 2, 12),
            DueDate: new DateOnly(2026, 3, 14),
            ManualAmount: true,
            ItemNet: false,
            LineItems: [new BillLineItem(Position: 0, Amount: 56.80m)],
            Discounts: [],
            AttachmentIds: []);

        var result = await service.Create(payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data!.Id, Is.EqualTo(TestBillId));
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(BillsPath));
            Assert.That(request.Body, Does.Contain("\"currency_code\":\"USD\""));
            Assert.That(request.Body, Does.Contain("\"supplier_id\":1323"));
            Assert.That(request.Body, Does.Contain("\"contact_partner_id\":647"));
            Assert.That(request.Body, Does.Contain("\"manual_amount\":true"));
            Assert.That(request.Body, Does.Contain("\"item_net\":false"));
        });
    }

    /// <summary>
    /// <c>BillService.Actions</c> sends a <c>POST</c> request to
    /// <c>/4.0/purchase/bills/{id}/actions</c> with the action payload, and the
    /// 200 response deserializes into the canonical <see cref="Bill"/> (the duplicate).
    /// </summary>
    [Test]
    public async Task BillService_Actions_SendsPostRequestToActionsPath()
    {
        var expectedPath = $"{BillsPath}/{TestBillId}/actions";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FullBillResponse));

        var service = new BillService(ConnectionHandler);

        var result = await service.Actions(
            TestBillId,
            new BillActionRequest(BillAction.DUPLICATE),
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("POST"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"action\":\"DUPLICATE\""));
        });
    }

    /// <summary>
    /// <c>BillService.Update</c> sends a <c>PUT</c> request against
    /// <c>/4.0/purchase/bills/{id}</c> per the v3.0.0 OpenAPI spec
    /// (<see href="https://docs.bexio.com/#tag/Bills/operation/ApiBills_PUT" />).
    /// </summary>
    [Test]
    public async Task BillService_Update_SendsPutRequestWithIdInPath()
    {
        var expectedPath = $"{BillsPath}/{TestBillId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FullBillResponse));

        var service = new BillService(ConnectionHandler);

        var payload = new BillUpdate(
            SupplierId: 1323,
            ContactPartnerId: 647,
            CurrencyCode: "CHF",
            Address: new BillAddress("Acme", BillAddressType.COMPANY),
            BillDate: new DateOnly(2026, 2, 12),
            DueDate: new DateOnly(2026, 3, 14),
            ManualAmount: false,
            ItemNet: true,
            SplitIntoLineItems: true,
            LineItems: [new BillLineItem(Position: 0, Amount: 56.8m)],
            Discounts: [],
            AttachmentIds: []);

        var result = await service.Update(TestBillId, payload, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
            Assert.That(request.Body, Does.Contain("\"split_into_line_items\":true"));
        });
    }

    /// <summary>
    /// <c>BillService.UpdateBookings</c> sends a <c>PUT</c> request against
    /// <c>/4.0/purchase/bills/{id}/bookings/{status}</c> with the target status
    /// in the URL — the body is intentionally empty.
    /// </summary>
    [Test]
    public async Task BillService_UpdateBookings_SendsPutRequestWithStatusInPath()
    {
        var expectedPath = $"{BillsPath}/{TestBillId}/bookings/BOOKED";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingPut())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(FullBillResponse));

        var service = new BillService(ConnectionHandler);

        var result = await service.UpdateBookings(
            TestBillId,
            BillBookingStatus.BOOKED,
            TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(request.Method, Is.EqualTo("PUT"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }

    /// <summary>
    /// <c>BillService.Delete</c> issues a <c>DELETE</c> request that includes the
    /// bill id in the URL path and accepts the spec's empty <c>204</c> response body.
    /// </summary>
    [Test]
    public async Task BillService_Delete_SendsDeleteRequestWithIdInPath()
    {
        var expectedPath = $"{BillsPath}/{TestBillId}";

        Server
            .Given(Request.Create().WithPath(expectedPath).UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(204));

        var service = new BillService(ConnectionHandler);

        var result = await service.Delete(TestBillId, TestContext.CurrentContext.CancellationToken);

        var request = Server.LogEntries.Last().RequestMessage!;

        Assert.Multiple(() =>
        {
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(request.Method, Is.EqualTo("DELETE"));
            Assert.That(request.AbsolutePath, Is.EqualTo(expectedPath));
        });
    }
}
