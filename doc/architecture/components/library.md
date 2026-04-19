---
title: Library Components
tags: [architecture, c4, components]
---

# Internal Library Components: BexioApiNet Core

This diagram details the internal structure of the `BexioApiNet` implementation project and how requests are processed.

## C4 Component Diagram

```mermaid
C4Component
  title Component diagram for BexioApiNet Core

  Container_Boundary(core, "BexioApiNet") {
    Component(bexioApiClient, "BexioApiClient", "Facade", "Aggregates all connector services. Main entry point for developers.")
    
    Component(accountingConnectors, "Accounting Services", "ConnectorService", "Handles ManualEntries, Accounts, Currencies, Taxes.")
    Component(bankingConnectors, "Banking Services", "ConnectorService", "Handles BankAccounts.")
    Component(salesConnectors, "Sales Services", "ConnectorService", "Handles Invoices, Quotes, Orders, Deliveries.")
    
    Component(connectionHandler, "BexioConnectionHandler", "HTTP execution", "Manages HttpClient, Bearer token injection, request building, pagination, and response deserialization.")
  }

  Container(abstractions, "BexioApiNet.Abstractions", "Domain Models", "ApiResult<T>, QueryParameters, etc.")
  System_Ext(bexioApi, "Bexio REST API", "Upstream API")

  Rel(bexioApiClient, accountingConnectors, "Delegates to")
  Rel(bexioApiClient, bankingConnectors, "Delegates to")
  Rel(bexioApiClient, salesConnectors, "Delegates to")
  
  Rel(accountingConnectors, connectionHandler, "Uses for HTTP")
  Rel(bankingConnectors, connectionHandler, "Uses for HTTP")
  Rel(salesConnectors, connectionHandler, "Uses for HTTP")
  
  Rel(connectionHandler, bexioApi, "Executes HTTP requests")
  Rel(connectionHandler, abstractions, "Deserializes into models")
```

## Component Breakdown

| Component | Responsibility | Path |
|-----------|----------------|------|
| **BexioApiClient** | Facade implementing `IBexioApiClient`. Provides convenient properties (e.g., `AccountingManualEntries`, `BankingBankAccounts`) to access specific service areas. | `src/BexioApiNet/Services/BexioApiClient.cs` |
| **Connector Services** | Concrete services inheriting from `ConnectorService` (e.g., `ManualEntryService`, `BankAccountService`) or the shared `PositionService` (for the 7 `kb_position_*` types). They construct specific API paths (using structures like `ManualEntryConfiguration`) and define typed methods for standard verbs and extended actions. | `src/BexioApiNet/Services/Connectors/` |
| **BexioConnectionHandler** | Implements `IBexioConnectionHandler`. Wraps `HttpClient`. Applies authorization and accept headers. Contains robust methods for full CRUD (GET, POST, PUT, PATCH, DELETE), file uploads, binary downloads, actions, bulk operations, searches, and auto-paginating through collection endpoints. | `src/BexioApiNet/Services/BexioConnectionHandler.cs` |
| **Query Parameters** | Wrappers for dictionary-based optional URL queries. Handled systematically by the `BexioConnectionHandler` during URL construction. | `src/BexioApiNet/Models/` |
