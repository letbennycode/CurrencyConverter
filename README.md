# Wex Currency Product Demo

A REST API that stores purchase transactions in USD and converts them to a target currency using exchange rates published by the [U.S. Treasury Fiscal Data API](https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange). Purchases are persisted to a local SQLite database. On retrieval, the API looks up the most recent exchange rate published within six months of the transaction date and returns the converted amount alongside the original transaction details.

---

## Quickstart

**Prerequisites:** .NET 10 SDK

```bash
git clone https://github.com/letbennycode/CurrencyConverter.git
cd Wex-Currency-Product-Demo
dotnet run --project src/WexCurrencyConverter.Api
```

Swagger UI is available at `http://localhost:5038/swagger` once the app is running in Development mode.

---

## API

### POST /api/purchase

Store a new purchase transaction.

**Request**

```bash
curl -X POST http://localhost:5038/api/purchase \
  -H "Content-Type: application/json" \
  -d '{
    "description": "Coffee Run",
    "transactionDate": "2026-01-15",
    "amountUsd": 67.99
  }'
```

**Response** `201 Created`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Coffee Run",
  "transactionDate": "2026-01-15",
  "amountUsd": 67.99
}
```

**Validation rules:**

- `description` is required, max 50 characters (leading/trailing whitespace is trimmed before the limit is checked)
- `transactionDate` cannot be in the future
- `amountUsd` must be a positive value with no more than two decimal places

Violations return `422 Unprocessable Entity` alongside a custom exception.

---

### GET /api/purchase/{id}?currency={currency}

Retrieve a stored purchase and convert it to the target currency.

**Request**

```bash
curl "http://localhost:5038/api/purchase/3fa85f64-5717-4562-b3fc-2c963f66afa6?currency=Canada-Dollar"
```

**Response** `200 OK`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Coffee Run",
  "transactionDate": "2026-01-15",
  "amountUsd": 67.99,
  "currency": "Canada-Dollar",
  "exchangeRate": 1.326,
  "exchangeRateDate": "2026-12-31",
  "convertedAmount": 66.29
}
```

**Currency format:** The `currency` parameter must match the Treasury's `country_currency_desc` field exactly, for example `Canada-Dollar`, `Euro Zone-Euro`, `Japan-Yen` as described in the external API documentation for the treasury API.

**Error responses:**

- `404` if the purchase ID does not exist
- `422` if no exchange rate is available within 6 months before the transaction date
- `503` if the Treasury API circuit breaker is open

---

## Architecture

```
src/
  WexCurrencyConverter.Api            # HTTP layer: controllers, middleware, DI wiring
  WexCurrencyConverter.Application    # Use cases, service interfaces, request/response models
  WexCurrencyConverter.Domain         # Purchase entity, domain validation, domain exceptions
  WexCurrencyConverter.Infrastructure # EF Core, SQLite, Treasury HTTP client, caching

tests/
  WexCurrencyConverter.UnitTests         # Fast, isolated tests with mocked dependencies
  WexCurrencyConverter.IntegrationTests  # Full-stack tests against a real HTTP server
```

The project follows a clean/layered architecture. Domain has no dependencies on anything else. Application depends only on Domain. Infrastructure depends on Application (to implement its interfaces). The API layer wires everything together.

**Middleware pipeline**

```
Request  -->  ExceptionHandler  -->  HTTPS  -->  Controllers
Response <--  ExceptionHandler  <--  HTTPS  <--  Controllers
                    |
          middleware catches here
```

`ApiExceptionHandler` maps domain exceptions to appropriate HTTP status codes in one place, keeping controllers free of try/catch blocks.

---

## Design Decisions

### SQLite

SQLite is embedded and file-based. There is no server to install or configure, the database file survives restarts. In-memory is used for Integration Tests to prevent test DBs from growing large.

### Controllers vs. minimal APIs

My current C# experience lended itself to Controllers, these were chosen for familiarity and because `[ProducesResponseType]` attributes make Swagger output explicit. The contract is documented at the declaration site rather than inferred.

### `sealed` classes

Service and exception classes are marked `sealed` where there is no inheritance needed.

### `decimal` for money

`double` and `float` cannot represent most decimal fractions exactly. `decimal` is base-10 and handles currency arithmetic without surprises.

### `DateOnly` for dates

Transactions have a date, not a point in time. Using `DateTime` would require picking a time zone and converting at every boundary. `DateOnly` removes that problem entirely. Tests could also run into issues depending on the time ran, so DateOnly enables a smoother testing process for this use case.

### Rounding mode

Converted amounts are rounded using `MidpointRounding.ToEven`. When the result is exactly halfway between two cents, it rounds to the nearest even digit rather than always rounding up. Example: `$100.00 * 1.005 = $100.50` (rounds to even).

### Caching

Treasury Reporting Rates of Exchange are published quarterly and do not change intra-day. Exchange rate lookups are cached in-memory (`IMemoryCache`) for 1 hour, which helps in a production use case when hitting costly external APIs and improving performance. A 1-hour TTL is safe for production use.

The cache key includes currency, the transaction date, and the 6-month lookback date, so different windows get separate cache entries.

### Resilience

The `HttpClient` for the Treasury API is configured with `AddStandardResilienceHandler`, which covers:

- **Retries** on 5xx responses and `HttpRequestException`
- **Per-attempt timeout** to prevent one slow request from blocking indefinitely
- **Circuit breaker** that opens after a failure threshold is crossed, returning `503` immediately instead of waiting on a degraded upstream

### `422` for unprocurable rates

When no exchange rate exists within 6 months of the transaction date, the API returns `422 Unprocessable Entity`. The request was syntactically valid and the record exists, but the conversion cannot be completed. `404` would imply the purchase was not found, which is not the case. `422` communicates that the server understood the request but could not process it due to business rule conditions.

## Working Assumptions

- **Currency identifier format.** The Treasury Fiscal Data API identifies currencies by `country_currency_desc` (e.g. `Canada-Dollar`). This API accepts and passes through that format directly. However, this could be mapped to ISO codes in the future.

- **Transaction date validation.** Dates cannot be in the future. There is no lower bound on how far back a date can go. A purchase stored with a date older than 6 months will be saved successfully, but any conversion request against it will return `422` because no rate will be found in the 6-month window. This separates storage concerns from conversion concerns.

---

## Running Tests

```bash
# Unit tests only
dotnet test tests/WexCurrencyConverter.UnitTests

# Integration tests only
dotnet test tests/WexCurrencyConverter.IntegrationTests

# Everything
dotnet test
```

### What is tested

**Unit tests** (`NSubstitute` + `Moq`) mock at the interface. `ITreasuryRatesClient` is substituted, so none of the real HTTP client code runs. These tests cover orchestration logic: correct date window passed to the client, null rate handling, rounding behavior, domain validation rules.

**Integration tests** (`WireMock.Net`) mock at the HTTP boundary. The full stack runs end to end against a real in-process .NET Core server. WireMock stands in for the Treasury API and returns fixture JSON from `TestData/`.

---

## What I Would Do Next

- Right now, submitting the same request twice creates two records. An idempotency key header or a unique constraint on (description, date, amount) would prevent accidental duplicates.
- There is no GET-all currently. Adding it with cursor-based or offset pagination would be a natural next step.
- No auth is enforced. Adding API key or JWT-based auth would be the minimum for any external exposure.
- Add structured logging with correlation IDs, request duration metrics, and tracing with telemetry. An Azure App Insights Resource would be useful in the case of needing to triage.
- The 6-month window means purchases older than 6 months can never be converted. Options: find a data source with fuller history, or prevent storing purchases with dates that far back rather than surfacing the error when trying to convert.
- The Treasury base URL is not a secret, but any API key or connection string for a real database should come from environment variables or a secrets manager like Azure Key Vault, not `appsettings.json`.
- Switch to an actual relational
- A relational DB like SQL would make sense for this, storing transactions with a well-defined schema. I could see CosmosDB being useful in the case of high-write throughput or the need for extremely low latency on a global system, but that's not needed here especially in a small production use case.