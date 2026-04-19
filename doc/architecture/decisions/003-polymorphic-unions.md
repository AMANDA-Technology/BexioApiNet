# ADR 003: Polymorphic Unions for API Types

**Status:** Accepted  
**Date:** 2026-04-19

## Context
The Bexio API uses polymorphic types in several places, such as positions (`anyOf` over `kb_position_*`) and order repetitions (`oneOf` over schedule types). System.Text.Json does not natively support deserializing these discriminators into a unified base type out-of-the-box in a zero-configuration way without custom converters. Initially, we mapped these to `JsonElement` leaving the parsing up to the consumer, which defeats the purpose of a strongly-typed client.

## Decision
We model polymorphic unions as an abstract base `record` with sealed concrete derived `record`s. We implement a custom `DiscriminatedJsonConverter<TBase>` that inspects a discriminator field (usually `type`) and delegates to the appropriate concrete type. The `[JsonConverter]` attribute is applied only to the base abstract record.

## Consequences
- Consumers get a fully strongly-typed object model without having to parse raw JSON or work with `JsonElement`.
- We avoid dependencies on third-party JSON libraries (e.g., Newtonsoft.Json), remaining strictly on `System.Text.Json` as required by our architectural principles.
- Adding new subtypes requires adding a new record and updating the switch statement in the relevant converter subclass.
- Response consumers that formerly enumerated over `JsonElement` properties will need to adapt to pattern matching on the concrete subtype (e.g. `switch (position) { case PositionArticle a: ... }`).
