---
title: API Completeness Audit
created: 2026-04-19
source: doc/openapi/bexio-v3.json (retrieved 2026-04-18)
status: tracking
---

# API Completeness Audit — BexioApiNet

Full inventory of the Bexio REST API v3.0.0 mapped against BexioApiNet implementation status. Source of truth: vendored OpenAPI spec at `doc/openapi/bexio-v3.json`.

**Total endpoint methods:** 309  
**Implemented:** 223 (72.2%)  
**Remaining:** 86 (27.8%)

---

## Coverage Summary by Domain Group

| # | Domain Group | Tags | Endpoints | Done | Remaining | Wave |
|---|---|---|---|---|---|---|
| 1 | Accounting (Extended) | 8 | 35 | 35 | 0 | 1 |
| 2 | Banking & Payments | 4 | 15 | 15 | 0 | 1 |
| 3 | Contacts & CRM | 5 | 28 | 28 | 0 | 1 |
| 4 | Sales — Invoices | 1 | 26 | 26 | 0 | 2 |
| 5 | Sales — Quotes | 1 | 17 | 17 | 0 | 2 |
| 6 | Sales — Orders & Deliveries | 2 | 15 | 15 | 0 | 2 |
| 7 | Items & Inventory | 4 | 16 | 16 | 0 | 3 |
| 8 | Document Positions | 7 | 35 | 35 | 0 | 3 |
| 9 | Projects | 1 | 20 | 0 | 20 | 4 |
| 10 | Timesheets & Tasks | 3 | 18 | 0 | 18 | 4 |
| 11 | Purchase & Expenses | 3 | 21 | 21 | 0 | 5 |
| 12 | Payroll | 3 | 10 | 10 | 0 | 5 |
| 13 | Files & Documents | 3 | 11 | 0 | 11 | 6 |
| 14 | Master Data & Settings | 9 | 42 | 0 | 42 | 6 |
| | **TOTAL** | **56** | **309** | **223** | **86** | |

---

## Domain Group 1: Accounting (Extended) — Wave 1 — DONE

### Tag: Accounts (2 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/accounts` | DONE | AccountService |
| POST | `/2.0/accounts/search` | DONE | AccountService |

### Tag: Account Groups (1 endpoint) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/account_groups` | DONE | AccountGroupService |

### Tag: Currencies (7 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/currencies` | DONE | CurrencyService |
| GET | `/3.0/currencies/codes` | DONE | CurrencyService |
| GET | `/3.0/currencies/{currency_id}` | DONE | CurrencyService |
| GET | `/3.0/currencies/{currency_id}/exchange_rates` | DONE | CurrencyService |
| POST | `/3.0/currencies` | DONE | CurrencyService |
| PATCH | `/3.0/currencies/{currency_id}` | DONE | CurrencyService |
| DELETE | `/3.0/currencies/{currency_id}` | DONE | CurrencyService |

### Tag: Manual Entries (13 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/manual_entries` | DONE | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/next_ref_nr` | DONE | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files` | DONE | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files/{file_id}` | DONE | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/files` | DONE | ManualEntryService |
| GET | `/3.0/accounting/manual_entries/{id}/files/{file_id}` | DONE | ManualEntryService |
| POST | `/3.0/accounting/manual_entries` | DONE | ManualEntryService |
| POST | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files` | DONE | ManualEntryService |
| POST | `/3.0/accounting/manual_entries/{id}/files` | DONE | ManualEntryService |
| PUT | `/3.0/accounting/manual_entries/{id}` | DONE | ManualEntryService |
| DELETE | `/3.0/accounting/manual_entries/{id}` | DONE | ManualEntryService |
| DELETE | `/3.0/accounting/manual_entries/{id}/entries/{entry_id}/files/{file_id}` | DONE | ManualEntryService |
| DELETE | `/3.0/accounting/manual_entries/{id}/files/{file_id}` | DONE | ManualEntryService |

### Tag: Taxes (3 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/taxes` | DONE | TaxService |
| GET | `/3.0/taxes/{tax_id}` | DONE | TaxService |
| DELETE | `/3.0/taxes/{tax_id}` | DONE | TaxService |

### Tag: Business Years (2 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/business_years` | DONE | BusinessYearService |
| GET | `/3.0/accounting/business_years/{id}` | DONE | BusinessYearService |

### Tag: Calendar Years (4 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/calendar_years` | DONE | CalendarYearService |
| GET | `/3.0/accounting/calendar_years/{id}` | DONE | CalendarYearService |
| POST | `/3.0/accounting/calendar_years` | DONE | CalendarYearService |
| POST | `/3.0/accounting/calendar_years/search` | DONE | CalendarYearService |

### Tag: Reports (1 endpoint) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/journal` | DONE | ReportService |

### Tag: Vat Periods (2 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/accounting/vat_periods` | DONE | VatPeriodService |
| GET | `/3.0/accounting/vat_periods/{id}` | DONE | VatPeriodService |

---

## Domain Group 2: Banking & Payments — Wave 1 — DONE

### Tag: Bank Accounts (2 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/banking/accounts` | DONE | BankAccountService |
| GET | `/3.0/banking/accounts/{bank_account_id}` | DONE | BankAccountService |

### Tag: Payment Types (2 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/payment_type` | DONE | PaymentTypeService |
| POST | `/2.0/payment_type/search` | DONE | PaymentTypeService |

### Tag: Payments (6 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/banking/payments` | DONE | PaymentService |
| GET | `/4.0/banking/payments/{payment_id}` | DONE | PaymentService |
| POST | `/4.0/banking/payments` | DONE | PaymentService |
| POST | `/4.0/banking/payments/{payment_id}/cancel` | DONE | PaymentService |
| PUT | `/4.0/banking/payments/{payment_id}` | DONE | PaymentService |
| DELETE | `/4.0/banking/payments/{payment_id}` | DONE | PaymentService |

### Tag: Outgoing Payments (5 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/purchase/outgoing-payments` | DONE | OutgoingPaymentService |
| GET | `/4.0/purchase/outgoing-payments/{id}` | DONE | OutgoingPaymentService |
| POST | `/4.0/purchase/outgoing-payments` | DONE | OutgoingPaymentService |
| PUT | `/4.0/purchase/outgoing-payments` | DONE | OutgoingPaymentService |
| DELETE | `/4.0/purchase/outgoing-payments/{id}` | DONE | OutgoingPaymentService |

---

## Domain Group 3: Contacts & CRM — Wave 1 — DONE

### Tag: Contacts (8 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact` | DONE | ContactService |
| GET | `/2.0/contact/{contact_id}` | DONE | ContactService |
| POST | `/2.0/contact` | DONE | ContactService |
| POST | `/2.0/contact/_bulk_create` | DONE | ContactService |
| POST | `/2.0/contact/search` | DONE | ContactService |
| POST | `/2.0/contact/{contact_id}` | DONE | ContactService |
| PATCH | `/2.0/contact/{contact_id}/restore` | DONE | ContactService |
| DELETE | `/2.0/contact/{contact_id}` | DONE | ContactService |

### Tag: Contact Groups (6 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact_group` | DONE | ContactGroupService |
| GET | `/2.0/contact_group/{id}` | DONE | ContactGroupService |
| POST | `/2.0/contact_group` | DONE | ContactGroupService |
| POST | `/2.0/contact_group/search` | DONE | ContactGroupService |
| POST | `/2.0/contact_group/{id}` | DONE | ContactGroupService |
| DELETE | `/2.0/contact_group/{id}` | DONE | ContactGroupService |

### Tag: Contact Relations (6 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact_relation` | DONE | ContactRelationService |
| GET | `/2.0/contact_relation/{id}` | DONE | ContactRelationService |
| POST | `/2.0/contact_relation` | DONE | ContactRelationService |
| POST | `/2.0/contact_relation/search` | DONE | ContactRelationService |
| POST | `/2.0/contact_relation/{id}` | DONE | ContactRelationService |
| DELETE | `/2.0/contact_relation/{id}` | DONE | ContactRelationService |

### Tag: Contact Sectors (2 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact_branch` | DONE | ContactSectorService |
| POST | `/2.0/contact_branch/search` | DONE | ContactSectorService |

### Tag: Additional Addresses (6 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/contact/{contact_id}/additional_address` | DONE | AdditionalAddressService |
| GET | `/2.0/contact/{contact_id}/additional_address/{id}` | DONE | AdditionalAddressService |
| POST | `/2.0/contact/{contact_id}/additional_address` | DONE | AdditionalAddressService |
| POST | `/2.0/contact/{contact_id}/additional_address/search` | DONE | AdditionalAddressService |
| POST | `/2.0/contact/{contact_id}/additional_address/{id}` | DONE | AdditionalAddressService |
| DELETE | `/2.0/contact/{contact_id}/additional_address/{id}` | DONE | AdditionalAddressService |

---

## Domain Group 4: Sales — Invoices — Wave 2 — DONE

### Tag: Invoices (26 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_invoice` | DONE | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}` | DONE | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/pdf` | DONE | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/payment` | DONE | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/payment/{payment_id}` | DONE | InvoiceService |
| GET | `/2.0/kb_invoice/{invoice_id}/kb_reminder` | DONE | InvoiceReminderService |
| GET | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}` | DONE | InvoiceReminderService |
| GET | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}/pdf` | DONE | InvoiceReminderService |
| POST | `/2.0/kb_invoice` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/search` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/issue` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/cancel` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/copy` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/send` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/mark_as_sent` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/revert_issue` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/payment` | DONE | InvoiceService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder` | DONE | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/search` | DONE | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{id}/send` | DONE | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{id}/mark_as_sent` | DONE | InvoiceReminderService |
| POST | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{id}/mark_as_unsent` | DONE | InvoiceReminderService |
| DELETE | `/2.0/kb_invoice/{invoice_id}` | DONE | InvoiceService |
| DELETE | `/2.0/kb_invoice/{invoice_id}/payment/{payment_id}` | DONE | InvoiceService |
| DELETE | `/2.0/kb_invoice/{invoice_id}/kb_reminder/{reminder_id}` | DONE | InvoiceReminderService |

---

## Domain Group 5: Sales — Quotes — Wave 2 — DONE

### Tag: Quotes (17 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_offer` | DONE | QuoteService |
| GET | `/2.0/kb_offer/{quote_id}` | DONE | QuoteService |
| GET | `/2.0/kb_offer/{quote_id}/pdf` | DONE | QuoteService |
| POST | `/2.0/kb_offer` | DONE | QuoteService |
| POST | `/2.0/kb_offer/search` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/issue` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/reissue` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/revertIssue` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/accept` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/reject` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/copy` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/invoice` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/order` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/mark_as_sent` | DONE | QuoteService |
| POST | `/2.0/kb_offer/{quote_id}/send` | DONE | QuoteService |
| DELETE | `/2.0/kb_offer/{quote_id}` | DONE | QuoteService |

---

## Domain Group 6: Sales — Orders & Deliveries — Wave 2 — DONE

### Tag: Orders (12 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_order` | DONE | OrderService |
| GET | `/2.0/kb_order/{order_id}` | DONE | OrderService |
| GET | `/2.0/kb_order/{order_id}/pdf` | DONE | OrderService |
| GET | `/2.0/kb_order/{order_id}/repetition` | DONE | OrderService |
| POST | `/2.0/kb_order` | DONE | OrderService |
| POST | `/2.0/kb_order/search` | DONE | OrderService |
| POST | `/2.0/kb_order/{order_id}` | DONE | OrderService |
| POST | `/2.0/kb_order/{order_id}/delivery` | DONE | OrderService |
| POST | `/2.0/kb_order/{order_id}/invoice` | DONE | OrderService |
| POST | `/2.0/kb_order/{order_id}/repetition` | DONE | OrderService |
| DELETE | `/2.0/kb_order/{order_id}` | DONE | OrderService |
| DELETE | `/2.0/kb_order/{order_id}/repetition` | DONE | OrderService |

### Tag: Deliveries (3 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/kb_delivery` | DONE | DeliveryService |
| GET | `/2.0/kb_delivery/{delivery_id}` | DONE | DeliveryService |
| POST | `/2.0/kb_delivery/{delivery_id}/issue` | DONE | DeliveryService |

---

## Domain Group 7: Items & Inventory — Wave 3 — DONE

### Tag: Items (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/article` | DONE | ItemService |
| GET | `/2.0/article/{article_id}` | DONE | ItemService |
| POST | `/2.0/article` | DONE | ItemService |
| POST | `/2.0/article/search` | DONE | ItemService |
| POST | `/2.0/article/{article_id}` | DONE | ItemService |
| DELETE | `/2.0/article/{article_id}` | DONE | ItemService |

### Tag: Units (6 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/unit` | DONE | UnitService |
| GET | `/2.0/unit/{unit_id}` | DONE | UnitService |
| POST | `/2.0/unit` | DONE | UnitService |
| POST | `/2.0/unit/search` | DONE | UnitService |
| POST | `/2.0/unit/{unit_id}` | DONE | UnitService |
| DELETE | `/2.0/unit/{unit_id}` | DONE | UnitService |

### Tag: Stock Areas (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/stock_place` | DONE | StockAreaService |
| POST | `/2.0/stock_place/search` | DONE | StockAreaService |

### Tag: Stock Locations (2 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/stock` | DONE | StockLocationService |
| POST | `/2.0/stock/search` | DONE | StockLocationService |

---

## Domain Group 8: Document Positions — Wave 3 — DONE

All position types share the same polymorphic pattern: CRUD on `/{kb_document_type}/{document_id}/kb_position_{type}/{position_id}`.

### Tag: Item Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_article` | DONE | ItemPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_article/{position_id}` | DONE | ItemPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_article` | DONE | ItemPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_article/{position_id}` | DONE | ItemPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_article/{position_id}` | DONE | ItemPositionService |

### Tag: Default Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_custom` | DONE | DefaultPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_custom/{position_id}` | DONE | DefaultPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_custom` | DONE | DefaultPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_custom/{position_id}` | DONE | DefaultPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_custom/{position_id}` | DONE | DefaultPositionService |

### Tag: Discount Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_discount` | DONE | DiscountPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_discount/{position_id}` | DONE | DiscountPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_discount` | DONE | DiscountPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_discount/{position_id}` | DONE | DiscountPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_discount/{position_id}` | DONE | DiscountPositionService |

### Tag: Text Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_text` | DONE | TextPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_text/{position_id}` | DONE | TextPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_text` | DONE | TextPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_text/{position_id}` | DONE | TextPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_text/{position_id}` | DONE | TextPositionService |

### Tag: Subtotal Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal` | DONE | SubtotalPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal/{position_id}` | DONE | SubtotalPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal` | DONE | SubtotalPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal/{position_id}` | DONE | SubtotalPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_subtotal/{position_id}` | DONE | SubtotalPositionService |

### Tag: Sub Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition` | DONE | SubPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition/{position_id}` | DONE | SubPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition` | DONE | SubPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition/{position_id}` | DONE | SubPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_subposition/{position_id}` | DONE | SubPositionService |

### Tag: Pagebreak Positions (5 endpoints)

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak` | DONE | PagebreakPositionService |
| GET | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak/{position_id}` | DONE | PagebreakPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak` | DONE | PagebreakPositionService |
| POST | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak/{position_id}` | DONE | PagebreakPositionService |
| DELETE | `/2.0/{kb_document_type}/{document_id}/kb_position_pagebreak/{position_id}` | DONE | PagebreakPositionService |

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

## Domain Group 11: Purchase & Expenses — Wave 5 — DONE

### Tag: Bills (8 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/purchase/bills` | DONE | BillService |
| GET | `/4.0/purchase/bills/{id}` | DONE | BillService |
| GET | `/4.0/purchase/documentnumbers/bills` | DONE | BillService |
| POST | `/4.0/purchase/bills` | DONE | BillService |
| POST | `/4.0/purchase/bills/{id}/actions` | DONE | BillService |
| PUT | `/4.0/purchase/bills/{id}` | DONE | BillService |
| PUT | `/4.0/purchase/bills/{id}/bookings/{status}` | DONE | BillService |
| DELETE | `/4.0/purchase/bills/{id}` | DONE | BillService |

### Tag: Purchase Orders (5 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/3.0/purchase_orders` | DONE | PurchaseOrderService |
| GET | `/3.0/purchase_orders/{purchase_order_id}` | DONE | PurchaseOrderService |
| POST | `/3.0/purchase_orders` | DONE | PurchaseOrderService |
| PUT | `/3.0/purchase_orders/{purchase_order_id}` | DONE | PurchaseOrderService |
| DELETE | `/3.0/purchase_orders/{purchase_order_id}` | DONE | PurchaseOrderService |

### Tag: Expenses (8 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/expenses` | DONE | ExpenseService |
| GET | `/4.0/expenses/{id}` | DONE | ExpenseService |
| GET | `/4.0/expenses/documentnumbers` | DONE | ExpenseService |
| POST | `/4.0/expenses` | DONE | ExpenseService |
| POST | `/4.0/expenses/{id}/actions` | DONE | ExpenseService |
| PUT | `/4.0/expenses/{id}` | DONE | ExpenseService |
| PUT | `/4.0/expenses/{id}/bookings/{status}` | DONE | ExpenseService |
| DELETE | `/4.0/expenses/{id}` | DONE | ExpenseService |

---

## Domain Group 12: Payroll — Wave 5 — DONE

### Tag: Employees (4 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/payroll/employees` | DONE | EmployeeService |
| GET | `/4.0/payroll/employees/{employeeId}` | DONE | EmployeeService |
| POST | `/4.0/payroll/employees` | DONE | EmployeeService |
| PATCH | `/4.0/payroll/employees/{employeeId}` | DONE | EmployeeService |

### Tag: Absences (5 endpoints) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/payroll/employees/{employeeId}/absences` | DONE | AbsenceService |
| GET | `/4.0/payroll/employees/{employeeId}/absences/{absenceId}` | DONE | AbsenceService |
| POST | `/4.0/payroll/employees/{employeeId}/absences` | DONE | AbsenceService |
| PUT | `/4.0/payroll/employees/{employeeId}/absences/{absenceId}` | DONE | AbsenceService |
| DELETE | `/4.0/payroll/employees/{employeeId}/absences/{absenceId}` | DONE | AbsenceService |

### Tag: Documents/Paystubs (1 endpoint) — DONE

| Method | Path | Status | Service |
|--------|------|--------|---------|
| GET | `/4.0/payroll/employees/{employeeId}/paystub-pdf/{year}/{month}` | DONE | PaystubService |

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
entages.
