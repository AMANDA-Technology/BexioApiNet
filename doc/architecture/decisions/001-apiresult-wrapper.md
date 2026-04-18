# ADR-001: ApiResult Wrapper Pattern

## Context
When interacting with the Bexio REST API, HTTP calls can fail for various reasons (404 Not Found, 400 Bad Request, 429 Too Many Requests, etc.). Bexio typically returns a structured JSON error body. Traditional .NET libraries might throw an `HttpRequestException` or custom exceptions upon encountering non-2xx status codes. However, exception-based flow control for expected HTTP states can be costly and requires developers to implement wide `try-catch` blocks. Furthermore, Bexio provides valuable rate-limiting and pagination headers with responses.

## Decision
All connector methods in the `BexioApiNet` SDK return an `ApiResult<T>` (or non-generic `ApiResult`) instead of throwing exceptions for API errors.

The `ApiResult<T>` record encapsulates:
- `IsSuccess` boolean mapping to the HTTP status.
- `StatusCode` containing the exact HTTP code.
- `Data` containing the deserialized generic model (if successful).
- `ApiError` containing the deserialized Bexio error model (if unsuccessful).
- `ResponseHeaders` containing extracted meta-information like `RequestLimit`, `AppliedOffset`, and `TotalResults`.

## Consequences
- **Pros:** Safer error handling without try-catch blocks. Easy access to rate-limit headers. Unified mechanism for both successful requests and structured error decoding.
- **Cons:** Developers must explicitly check `.IsSuccess` before accessing `.Data`, requiring slightly more boilerplate at the call site.
