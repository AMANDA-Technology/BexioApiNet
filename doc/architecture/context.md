---
title: System Context
tags: [architecture, c4, context]
---

# System Context: BexioApiNet

BexioApiNet acts as a bridge between .NET applications and the Bexio REST API, encapsulating HTTP communication, authentication, and error handling.

## C4 System Context Diagram

```mermaid
C4Context
  title System Context diagram for BexioApiNet

  Person(developer, "Developer / .NET App", "A developer or a .NET application consuming the Bexio API")
  
  System(bexioApiNet, "BexioApiNet", ".NET SDK for Bexio API")
  System_Ext(bexioApi, "Bexio REST API", "The official Bexio REST API (v3.0.0) providing access to accounting, banking, contacts, etc.")

  Rel(developer, bexioApiNet, "Uses", "C# / .NET")
  Rel(bexioApiNet, bexioApi, "Makes API calls to", "HTTPS / JSON")
```

## Actors

| Actor | Description |
|-------|-------------|
| Developer / .NET App | Any third-party application or developer that needs to integrate with Bexio to manage accounting, taxes, or banking data. |

## External Systems

| System | Description |
|--------|-------------|
| Bexio REST API | The upstream SaaS platform API that manages the actual business data. |
