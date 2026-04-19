---
title: API Completeness Audit
created: 2026-04-19
source: doc/openapi/bexio-v3.json (retrieved 2026-04-18)
status: tracking
---

# API Completeness Audit — BexioApiNet

Full inventory of the Bexio REST API v3.0.0 mapped against BexioApiNet implementation status. Source of truth: vendored OpenAPI spec at `doc/openapi/bexio-v3.json`.

**Total endpoint methods:** 309  
**Implemented:** 15 (4.8%)  
**Remaining:** 294 (95.2%)

---

## Coverage Summary by Domain Group

| # | Domain Group | Tags | Endpoints | Done | Remaining | Wave |
|---|---|---|---|---|---|---|
| 1 | Accounting (Extended) | 8 | 35 | 10 | 25 | 1 |
| 2 | Banking & Payments | 4 | 15 | 1 | 14 | 1 |
| 3 | Contacts & CRM | 5 | 28 | 0 | 28 | 1 |
| 4 | Sales — Invoices | 1 | 26 | 0 | 26 | 2 |
| 5 | Sales — Quotes | 1 | 17 | 0 | 17 | 2 |
| 6 | Sales — Orders & Deliveries | 2 | 15 | 0 | 15 | 2 |
| 7 | Items & Inventory | 4 | 16 | 0 | 16 | 3 |
| 8 | Document Positions | 7 | 35 | 0 | 35 | 3 |
| 9 | Projects | 1 | 20 | 0 | 20 | 4 |
| 10 | Timesheets & Tasks | 3 | 18 | 0 | 18 | 4 |
| 11 | Purchase & Expenses | 3 | 21 | 0 | 21 | 5 |
| 12 | Payroll | 3 | 10 | 0 | 10 | 5 |
| 13 | Files & Documents | 3 | 11 | 0 | 11 | 6 |
| 14 | Master Data & Settings | 9 | 42 | 0 | 42 | 6 |
| | **TOTAL** | **56** | **309** | **15** | **294** | |

---

## Domain Group 1: Accounting (Extended) — Wave 1

### Tag: Accounts (2 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/accounts` | DONE | AccountService |
| POST | `/2.0/accounts/search` | DONE | AccountService |

### Tag: Account Groups (1 endpoint)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/account_groups` | TODO | AccountGroupService |

### Tag: Currencies (7 endpoints) — PARTIAL (1/7)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/currencies` | DONE | CurrencyService |
| GET | `/3.0/currencies/codes` | TODO | CurrencyService |
| GET | `/3.0/currencies/{currency_id}` | TODO | CurrencyService |
| GET | `/3.0/currencies/{currency_id}/exchange_rates` | TODO | CurrencyService |
| POST | `/3.0/currencies` | TODO | CurrencyService |
| PATCH | `/3.0/currencies/{currency_id}` | TODO | CurrencyService |
| DELETE | `/3.0/currencies/{currency_id}` | TODO | CurrencyService |

### Tag: Manual Entries (13 endpoints) — PARTIAL (5/13)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/manual_entries` | DONE | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/next_ref_nr` | TODO | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files` | TODO | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files/{file_id}` | TODO | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/files` | TODO | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/files/{file_id}` | TODO | ManualEntryService |
| POST | `/3.0/accounting/manual_entries` | DONE | ManualEntryService |
| POST | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files` | DONE | ManualEntryService |
| POST | `/3.0/accounting/manual_entries/{id}/files` | DONE | ManualEntryService |
| PUT | `/3.0/accounting/manual_entries/{id}` | TODO | ManualEntryService |
| DELETE | `/3.0/accounting/manual_entries/{id}` | DONE | ManualEntryService |
| DELETE | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files/{file_id}` | TODO | ManualEntryService |
| DELETE | `/3.0/accounting/manual_entries/{id}/files/{file_id}` | TODO | ManualEntryService |

### Tag: Taxes (3 endpoints) — PARTIAL (1/3)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/taxes` | DONE | TaxService |
| GET | `/3.0/taxes/{tax_id}` | TODO | TaxService |
| DELETE | `/3.0/taxes/{tax_id}` | TODO | TaxService |

### Tag: Business Years (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/business_years` | TODO | BusinessYearService |
| GET | `/3.0/accounting/business_years/{id}` | TODO | BusinessYearService |

### Tag: Calendar Years (4 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/calendar_years` | TODO | CalendarYearService |
| GET | `/3.0/accounting/calendar_years/{id}` | TODO | CalendarYearService |
| POST | `/3.0/accounting/calendar_years` | TODO | CalendarYearService |
| POST | `/3.0/accounting/calendar_years/search` | TODO | CalendarYearService |

### Tag: Reports (1 endpoint)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/journal` | TODO | ReportService |

### Tag: Vat Periods (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/vat_periods` | TODO | VatPeriodService |
| GET | `/3.0/accounting/vat_periods/{id}` | TODO | VatPeriodService |

---

## Domain Group 2: Banking & Payments — Wave 1

### Tag: Bank Accounts (2 endpoints) — PARTIAL (1/2)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/banking/accounts` | DONE | BankAccountService |
| GET | `/3.0/banking/accounts/{bank_account_id}` | TODO | BankAccountService |

### Tag: Payment Types (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/payment_type` | TODO | PaymentTypeService |
| POST | `/2.0/payment_type/search` | TODO | PaymentTypeService |

### Tag: Payments (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/banking/payments` | TODO | PaymentService |
| GET | `/4.0/banking/payments/{payment_id}` | TODO | PaymentService |
| POST | `/4.0/banking/payments` | TODO | PaymentService |
| POST | `/4.0/banking/payments/{payment_id}/cancel` | TODO | PaymentService |
| PUT | `/4.0/banking/payments/{payment_id}` | TODO | PaymentService |
| DELETE | `/4.0/banking/payments/{payment_id}` | TODO | PaymentService |

### Tag: Outgoing Payments (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/purchase/outgoing-payments` | TODO | OutgoingPaymentService |
| GET | `/4.0/purchase/outgoing-payments/{id}` | TODO | OutgoingPaymentService |
| POST | `/4.0/purchase/outgoing-payments` | TODO | OutgoingPaymentService |
| PUT | `/4.0/purchase/outgoing-payments` | TODO | OutgoingPaymentService |
| DELETE | `/4.0/purchase/outgoing-payments/{id}` | TODO | OutgoingPaymentService |

---

## Domain Group 3: Contacts & CRM — Wave 1

### Tag: Contacts (8 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact` | TODO | ContactService |
| GET | `/2.0/contact/{contact_id}` | TODO | ContactService |
| POST | `/2.0/contact` | TODO | ContactService |
| POST | `/2.0/contact/_bulk_create` | TODO | ContactService |
| POST | `/2.0/contact/search` | TODO | ContactService |
| POST | `/2.0/contact/{contact_id}` | TODO | ContactService |
| PATCH | `/2.0/contact/{contact_id}/restore` | TODO | ContactService |
| DELETE | `/2.0/contact/{contact_id}` | TODO | ContactService |

### Tag: Contact Groups (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact_group` | TODO | ContactGroupService |
| GET | `/2.0/contact_group/{id}` | TODO | ContactGroupService |
| POST | `/2.0/contact_group` | TODO | ContactGroupService |
| POST | `/2.0/contact_group/search` | TODO | ContactGroupService |
| POST | `/2.0/contact_group/{id}` | TODO | ContactGroupService |
| DELETE | `/2.0/contact_group/{id}` | TODO | ContactGroupService |

### Tag: Contact Relations (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact_relation` | TODO | ContactRelationService |
| GET | `/2.0/contact_relation/{id}` | TODO | ContactRelationService |
| POST | `/2.0/contact_relation` | TODO | ContactRelationService |
| POST | `/2.0/contact_relation/search` | TODO | ContactRelationService |
| POST | `/2.0/contact_relation/{id}` | TODO | ContactRelationService |
| DELETE | `/2.0/contact_relation/{id}` | TODO | ContactRelationService |

### Tag: Contact Sectors (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact_branch` | TODO | ContactSectorService |
| POST | `/2.0/contact_branch/search` | TODO | ContactSectorService |

### Tag: Additional Addresses (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact/{contact_id}/additional_address` | TODO | AdditionalAddressService |
| GET | `/2.0/contact/{contact_id}/additional_address/{id}` | TODO | AdditionalAddressService |
| POST | `/2.0/contact/{contact_id}/additional_address` | TODO | AdditionalAddressService |
| POST | `/2.0/contact/{contact_id}/additional_address/search` | TODO | AdditionalAddressService |
| POST | `/2.0/contact/{contact_id}/additional_address/{id}` | TODO | AdditionalAddressService |
| DELETE | `/2.0/contact/{contact_id}/additional_address/{id}` | TODO | AdditionalAddressService |

---

## Domain Group 4: Sales — Invoices — Wave 2

### Tag: Invoices (26 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_invoice` | TODO | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}` | TODO | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/pdf` | TODO | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/payment` | TODO | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/payment/{payment_id}` | TODO | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/kb_reminder` | TODO | InvoiceReminderService |
| GET | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}` | TODO | InvoiceReminderService |
| GET | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}/pdf` | TODO | InvoiceReminderService |
| POST | `/2.0/kb_invoice` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/search` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/issue` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/cancel` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/copy` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/send` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/mark_as_sent` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/revert_issue` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/payment` | TODO | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder` | TODO | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/search` | TODO | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{id}/send` | TODO | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{id}/mark_as_sent` | TODO | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{id}/mark_as_unsent` | TODO | InvoiceReminderService |
| DELETE | `/2.0/kb_invoice/{invoice_id}` | TODO | InvoiceService |
| DELETE | `/2.0/kb_invoice/{invoice_id}/payment/{payment_id}` | TODO | InvoiceService |
| DELETE | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}` | TODO | InvoiceReminderService |

---

## Domain Group 5: Sales — Quotes — Wave 2

### Tag: Quotes (17 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_offer` | TODO | QuoteService |
| GET | `/2.0/kb_offer/{quote_id}` | TODO | QuoteService |
| GET | `/2.0/kb_offer/{quote_id}/pdf` | TODO | QuoteService |
| POST | `/2.0/kb_offer` | TODO | QuoteService |
| POST | `/2.0/kb_offer/search` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/issue` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/reissue` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/revertIssue` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/accept` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/reject` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/copy` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/invoice` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/order` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/mark_as_sent` | TODO | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/send` | TODO | QuoteService |
| DELETE | `/2.0/kb_offer/{quote_id}` | TODO | QuoteService |

---

## Domain Group 6: Sales — Orders & Deliveries — Wave 2

### Tag: Orders (12 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_order` | TODO | OrderService |
| GET | `/2.0/kb_order/{order_id}` | TODO | OrderService |
| GET | `/2.0/kb_order/{order_id}/pdf` | TODO | OrderService |
| GET | `/2.0/kb_order/{order_id}/repetition` | TODO | OrderService |
| POST | `/2.0/kb_order` | TODO | OrderService |
| POST | `/2.0/kb_order/search` | TODO | OrderService |
| POST | `/2.0/kb_order/{order_id}` | TODO | OrderService |
| POST | `/2.0/kb_order/{order_id}/delivery` | TODO | OrderService |
| POST | `/2.0/kb_order/{order_id}/invoice` | TODO | OrderService |
| POST | `/2.0/kb_order/{order_id}/repetition` | TODO | OrderService |
| DELETE | `/2.0/kb_order/{order_id}` | TODO | OrderService |
| DELETE | `/2.0/kb_order/{order_id}/repetition` | TODO | OrderService |

### Tag: Deliveries (3 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_delivery` | TODO | DeliveryService |
| GET | `/2.0/kb_delivery/{delivery_id}` | TODO | DeliveryService |
| POST | `/2.0/kb_delivery/{delivery_id}/issue` | TODO | DeliveryService |

---

## Domain Group 7: Items & Inventory — Wave 3

### Tag: Items (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/article` | TODO | ItemService |
| GET | `/2.0/article/{article_id}` | TODO | ItemService |
| POST | `/2.0/article` | TODO | ItemService |
| POST | `/2.0/article/search` | TODO | ItemService |
| POST | `/2.0/article/{article_id}` | TODO | ItemService |
| DELETE | `/2.0/article/{article_id}` | TODO | ItemService |

### Tag: Units (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/unit` | TODO | UnitService |
| GET | `/2.0/unit/{unit_id}` | TODO | UnitService |
| POST | `/2.0/unit` | TODO | UnitService |
| POST | `/2.0/unit/search` | TODO | UnitService |
| POST | `/2.0/unit/{unit_id}` | TODO | UnitService |
| DELETE | `/2.0/unit/{unit_id}` | TODO | UnitService |

### Tag: Stock Areas (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/stock_place` | TODO | StockAreaService |
| POST | `/2.0/stock_place/search` | TODO | StockAreaService |

### Tag: Stock Locations (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/stock` | TODO | StockLocationService |
| POST | `/2.0/stock/search` | TODO | StockLocationService |

---

## Domain Group 8: Document Positions — Wave 3

All position types share the same polymorphic pattern: CRUD on `/{kb_document_type}/{document_id}/kb_position_{type}/{position_id}`.

### Tag: Item Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_article` | TODO | ItemPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_article/{position_id}` | TODO | ItemPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_article` | TODO | ItemPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_article/{position_id}` | TODO | ItemPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_article/{position_id}` | TODO | ItemPositionService |

### Tag: Default Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_custom` | TODO | DefaultPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_custom/{position_id}` | TODO | DefaultPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_custom` | TODO | DefaultPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_custom/{position_id}` | TODO | DefaultPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_custom/{position_id}` | TODO | DefaultPositionService |

### Tag: Discount Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_discount` | TODO | DiscountPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_discount/{position_id}` | TODO | DiscountPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_discount` | TODO | DiscountPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_discount/{position_id}` | TODO | DiscountPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_discount/{position_id}` | TODO | DiscountPositionService |

### Tag: Text Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_text` | TODO | TextPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_text/{position_id}` | TODO | TextPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_text` | TODO | TextPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_text/{position_id}` | TODO | TextPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_text/{position_id}` | TODO | TextPositionService |

### Tag: Subtotal Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal` | TODO | SubtotalPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal/{position_id}` | TODO | SubtotalPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal` | TODO | SubtotalPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal/{position_id}` | TODO | SubtotalPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal/{position_id}` | TODO | SubtotalPositionService |

### Tag: Sub Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition` | TODO | SubPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition/{position_id}` | TODO | SubPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition` | TODO | SubPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition/{position_id}` | TODO | SubPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition/{position_id}` | TODO | SubPositionService |

### Tag: Pagebreak Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak` | TODO | PagebreakPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak/{position_id}` | TODO | PagebreakPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak` | TODO | PagebreakPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak/{position_id}` | TODO | PagebreakPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak/{position_id}` | TODO | PagebreakPositionService |

---

## Domain Group 9: Projects — Wave 4

### Tag: Projects (20 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/pr_project` | TODO | ProjectService |
| GET | `/2.0/pr_project/{project_id}` | TODO | ProjectService |
| GET | `/2.0/pr_project_state` | TODO | ProjectStateService |
| GET | `/2.0/pr_project_type` | TODO | ProjectTypeService |
| GET | `/3.0/projects/{project_id}/milestones` | TODO | MilestoneService |
| GET | `/3.0/projects/{project_id}/milestones/{milestone_id}` | TODO | MilestoneService |
| GET | `/3.0/projects/{project_id}/packages` | TODO | PackageService |
| GET | `/3.0/projects/{project_id}/packages/{package_id}` | TODO | PackageService |
| POST | `/2.0/pr_project` | TODO | ProjectService |
| POST | `/2.0/pr_project/search` | TODO | ProjectService |
| POST | `/2.0/pr_project/{project_id}` | TODO | ProjectService |
| POST | `/2.0/pr_project/{project_id}/archive` | TODO | ProjectService |
| POST | `/2.0/pr_project/{project_id}/reactivate` | TODO | ProjectService |
| POST | `/3.0/projects/{project_id}/milestones` | TODO | MilestoneService |
| POST | `/3.0/projects/{project_id}/milestones/{milestone_id}` | TODO | MilestoneService |
| POST | `/3.0/projects/{project_id}/packages` | TODO | PackageService |
| PATCH | `/3.0/projects/{project_id}/packages/{package_id}` | TODO | PackageService |
| DELETE | `/2.0/pr_project/{project_id}` | TODO | ProjectService |
| DELETE | `/3.0/projects/{project_id}/milestones/{milestone_id}` | TODO | MilestoneService |
| DELETE | `/3.0/projects/{project_id}/packages/{package_id}` | TODO | PackageService |

---

## Domain Group 10: Timesheets & Tasks — Wave 4

### Tag: Timesheets (7 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/timesheet` | TODO | TimesheetService |
| GET | `/2.0/timesheet/{timesheet_id}` | TODO | TimesheetService |
| GET | `/2.0/timesheet_status` | TODO | TimesheetStatusService |
| POST | `/2.0/timesheet` | TODO | TimesheetService |
| POST | `/2.0/timesheet/search` | TODO | TimesheetService |
| POST | `/2.0/timesheet/{timesheet_id}` | TODO | TimesheetService |
| DELETE | `/2.0/timesheet/{timesheet_id}` | TODO | TimesheetService |

### Tag: Tasks (8 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/task` | TODO | TaskService |
| GET | `/2.0/task/{task_id}` | TODO | TaskService |
| GET | `/2.0/todo_priority` | TODO | TaskPriorityService |
| GET | `/2.0/todo_status` | TODO | TaskStatusService |
| POST | `/2.0/task` | TODO | TaskService |
| POST | `/2.0/task/search` | TODO | TaskService |
| POST | `/2.0/task/{task_id}` | TODO | TaskService |
| DELETE | `/2.0/task/{task_id}` | TODO | TaskService |

### Tag: Business Activities (3 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/client_service` | TODO | BusinessActivityService |
| POST | `/2.0/client_service` | TODO | BusinessActivityService |
| POST | `/2.0/client_service/search` | TODO | BusinessActivityService |

---

## Domain Group 11: Purchase & Expenses — Wave 5

### Tag: Bills (8 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/purchase/bills` | TODO | BillService |
| GET | `/4.0/purchase/bills/{id}` | TODO | BillService |
| GET | `/4.0/purchase/documentnumbers/bills` | TODO | BillService |
| POST | `/4.0/purchase/bills` | TODO | BillService |
| POST | `/4.0/purchase/bills/{id}/actions` | TODO | BillService |
| PUT | `/4.0/purchase/bills/{id}` | TODO | BillService |
| PUT | `/4.0/purchase/bills/{id}/bookings/{status}` | TODO | BillService |
| DELETE | `/4.0/purchase/bills/{id}` | TODO | BillService |

### Tag: Purchase Orders (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/purchase_orders` | TODO | PurchaseOrderService |
| GET | `/3.0/purchase_orders/{purchase_order_id}` | TODO | PurchaseOrderService |
| POST | `/3.0/purchase_orders` | TODO | PurchaseOrderService |
| PUT | `/3.0/purchase_orders/{purchase_order_id}` | TODO | PurchaseOrderService |
| DELETE | `/3.0/purchase_orders/{purchase_order_id}` | TODO | PurchaseOrderService |

### Tag: Expenses (8 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/expenses` | TODO | ExpenseService |
| GET | `/4.0/expenses/{id}` | TODO | ExpenseService |
| GET | `/4.0/expenses/documentnumbers` | TODO | ExpenseService |
| POST | `/4.0/expenses` | TODO | ExpenseService |
| POST | `/4.0/expenses/{id}/actions` | TODO | ExpenseService |
| PUT | `/4.0/expenses/{id}` | TODO | ExpenseService |
| PUT | `/4.0/expenses/{id}/bookings/{status}` | TODO | ExpenseService |
| DELETE | `/4.0/expenses/{id}` | TODO | ExpenseService |

---

## Domain Group 12: Payroll — Wave 5

### Tag: Employees (4 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/payroll/employees` | TODO | EmployeeService |
| GET | `/4.0/payroll/employees/{employeeId}` | TODO | EmployeeService |
| POST | `/4.0/payroll/employees` | TODO | EmployeeService |
| PATCH | `/4.0/payroll/employees/{employeeId}` | TODO | EmployeeService |

### Tag: Absences (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/payroll/employees/{employeeId}/absences` | TODO | AbsenceService |
| GET | `/4.0/payroll/employees/{employeeId}/absences/{absenceId}` | TODO | AbsenceService |
| POST | `/4.0/payroll/employees/{employeeId}/absences` | TODO | AbsenceService |
| PUT | `/4.0/payroll/employees/{employeeId}/absences/{absenceId}` | TODO | AbsenceService |
| DELETE | `/4.0/payroll/employees/{employeeId}/absences/{absenceId}` | TODO | AbsenceService |

### Tag: Documents/Paystubs (1 endpoint)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}` | TODO | PaystubService |

---

## Domain Group 13: Files & Documents — Wave 6

### Tag: Files (9 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/files` | TODO | FileService |
| GET | `/3.0/files/{file_id}` | TODO | FileService |
| GET | `/3.0/files/{file_id}/download` | TODO | FileService |
| GET | `/3.0/files/{file_id}/preview` | TODO | FileService |
| GET | `/3.0/files/{file_id}/usage` | TODO | FileService |
| POST | `/3.0/files` | TODO | FileService |
| POST | `/3.0/files/search` | TODO | FileService |
| PATCH | `/3.0/files/{file_id}` | TODO | FileService |
| DELETE | `/3.0/files/{file_id}` | TODO | FileService |

### Tag: Document Settings (1 endpoint)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_item_setting` | TODO | DocumentSettingService |

### Tag: Document Templates (1 endpoint)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/document_templates` | TODO | DocumentTemplateService |

---

## Domain Group 14: Master Data & Settings — Wave 6

### Tag: Countries (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/country` | TODO | CountryService |
| GET | `/2.0/country/{country_id}` | TODO | CountryService |
| POST | `/2.0/country` | TODO | CountryService |
| POST | `/2.0/country/search` | TODO | CountryService |
| POST | `/2.0/country/{country_id}` | TODO | CountryService |
| DELETE | `/2.0/country/{country_id}` | TODO | CountryService |

### Tag: Languages (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/language` | TODO | LanguageService |
| POST | `/2.0/language/search` | TODO | LanguageService |

### Tag: Salutations (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/salutation` | TODO | SalutationService |
| GET | `/2.0/salutation/{salutation_id}` | TODO | SalutationService |
| POST | `/2.0/salutation` | TODO | SalutationService |
| POST | `/2.0/salutation/search` | TODO | SalutationService |
| POST | `/2.0/salutation/{salutation_id}` | TODO | SalutationService |
| DELETE | `/2.0/salutation/{salutation_id}` | TODO | SalutationService |

### Tag: Titles (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/title` | TODO | TitleService |
| GET | `/2.0/title/{title_id}` | TODO | TitleService |
| POST | `/2.0/title` | TODO | TitleService |
| POST | `/2.0/title/search` | TODO | TitleService |
| POST | `/2.0/title/{title_id}` | TODO | TitleService |
| DELETE | `/2.0/title/{title_id}` | TODO | TitleService |

### Tag: Communication Types (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/communication_kind` | TODO | CommunicationTypeService |
| POST | `/2.0/communication_kind/search` | TODO | CommunicationTypeService |

### Tag: Company Profile (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/company_profile` | TODO | CompanyProfileService |
| GET | `/2.0/company_profile/{profile_id}` | TODO | CompanyProfileService |

### Tag: User Management (8 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/users` | TODO | UserService |
| GET | `/3.0/users/me` | TODO | UserService |
| GET | `/3.0/users/{user_id}` | TODO | UserService |
| GET | `/3.0/fictional_users` | TODO | FictionalUserService |
| GET | `/3.0/fictional_users/{fictional_user_id}` | TODO | FictionalUserService |
| POST | `/3.0/fictional_users` | TODO | FictionalUserService |
| PATCH | `/3.0/fictional_users/{fictional_user_id}` | TODO | FictionalUserService |
| DELETE | `/3.0/fictional_users/{fictional_user_id}` | TODO | FictionalUserService |

### Tag: Permissions (1 endpoint)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/permissions` | TODO | PermissionService |

### Tag: Notes (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/note` | TODO | NoteService |
| GET | `/2.0/note/{note_id}` | TODO | NoteService |
| POST | `/2.0/note` | TODO | NoteService |
| POST | `/2.0/note/search` | TODO | NoteService |
| POST | `/2.0/note/{note_id}` | TODO | NoteService |
| DELETE | `/2.0/note/{note_id}` | TODO | NoteService |

### Tag: Comments (3 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/comment` | TODO | CommentService |
| GET | `/2.0/{kb_document_type}/{document_id}/comment/{comment_id}` | TODO | CommentService |
| POST | `/2.0/{kb_document_type}/{document_id}/comment` | TODO | CommentService |

---

## Infrastructure Patterns Required

Before Wave 1 implementation, the following HTTP patterns must be supported by `BexioConnectionHandler`. Phase 0 (issue #23) delivered the remaining verbs and the shared `PositionService` base; all patterns are now in place.

| Pattern | Current Support | Example Endpoint | Action Needed |
|---------|----------------|------------------|---------------|
| GET (list + single) | YES | `/2.0/accounts` | None |
| POST (create) | YES | `/3.0/accounting/manual_entries` | None |
| POST (search) | YES | `/2.0/accounts/search` | None |
| POST (action) | YES | `/2.0/kb_invoice/{id}/issue` | None |
| PUT (update) | YES | `/3.0/accounting/manual_entries/{id}` | None |
| PATCH (partial update) | YES | `/3.0/currencies/{id}` | None |
| DELETE | YES | `/3.0/accounting/manual_entries/{id}` | None |
| GET (binary/PDF) | YES | `/2.0/kb_invoice/{id}/pdf` | None |
| POST (multipart file) | YES | `.../files` | None |
| POST (bulk) | YES | `/2.0/contact/_bulk_create` | None |

---

## Projected Service Count

| Domain Group | New Services | New Endpoint Methods |
|---|---|---|
| 1. Accounting Extended | 4 new + 4 expanded | 25 |
| 2. Banking & Payments | 3 new + 1 expanded | 14 |
| 3. Contacts & CRM | 5 new | 28 |
| 4. Sales — Invoices | 2 new | 26 |
| 5. Sales — Quotes | 1 new | 17 |
| 6. Sales — Orders | 2 new | 15 |
| 7. Items & Inventory | 4 new | 16 |
| 8. Document Positions | 7 new | 35 |
| 9. Projects | 4 new | 20 |
| 10. Timesheets & Tasks | 4 new | 18 |
| 11. Purchase & Expenses | 3 new | 21 |
| 12. Payroll | 3 new | 10 |
| 13. Files & Documents | 3 new | 11 |
| 14. Master Data | 10 new | 42 |
| **TOTAL** | **~55 new services** | **294 new methods** |

**Final state:** ~60 services, 309 endpoint methods, 100% API coverage.

---

## Update Protocol

When an endpoint is implemented:
1. Change its status from `TODO` to `DONE` in this document.
2. Update the summary table counts.
3. Update `doc/ai-readiness.md` coverage percentages.
