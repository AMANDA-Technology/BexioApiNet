---
title: Glossary
tags: [architecture, glossary]
---

# Domain Glossary

This glossary defines terms specific to the BexioApiNet domain and the Bexio API.

| Term | Definition |
|------|------------|
| **Manual Entry** | A journal entry in accounting (`Accounting/ManualEntries`). Represents an explicit booking against accounts, containing multiple sub-entries. |
| **ApiResult** | The wrapper class used consistently across BexioApiNet to encapsulate API responses, errors, HTTP status codes, and rate-limit headers. |
| **Connector** | A localized service class (e.g., `ManualEntryService`) dedicated to a specific set of endpoints within the Bexio API, responsible for defining the routes and strongly-typed request/response models. |
| **BexioConnectionHandler** | The internal component in the library responsible for processing raw HTTP requests, injecting authentication, handling offset-based auto-pagination (`FetchAll`), and executing multi-part form uploads. |
