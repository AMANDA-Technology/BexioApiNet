# ADR-002: Service Connector Partitioning

## Context
The Bexio REST API covers a vast domain including Accounting, Banking, Contacts, Invoices, Orders, Projects, and more. Creating a single monolithic client class (e.g., `BexioClient`) with hundreds of methods would lead to an unmaintainable "God class" that violates the Single Responsibility Principle and is difficult to test.

## Decision
We implemented a Service Connector pattern. The API is divided into logical namespaces (e.g., `Accounting`, `Banking`) mirroring Bexio's own domain structure. 

- An abstract base class `ConnectorService` holds common dependencies (like the `BexioConnectionHandler`).
- Each logical endpoint gets its own dedicated service implementation (e.g., `ManualEntryService`, `AccountService`).
- A central facade `BexioApiClient` aggregates these services into a single discoverable interface (`IBexioApiClient`) for easy consumer access (e.g., `bexio.AccountingManualEntries.Create(...)`).

## Consequences
- **Pros:** High cohesion. Easy to scale and maintain as the Bexio API grows. Logical grouping makes it easier for consumers via IntelliSense. Separation of concerns.
- **Cons:** Increased class count. Dependency Injection registration (in `BexioServiceCollection`) requires adding every new connector explicitly.
