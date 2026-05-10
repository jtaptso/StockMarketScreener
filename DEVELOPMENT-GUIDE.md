# Stock Market Screener — Development Guide

**Stack:** .NET 10 | Blazor WebAssembly | SQL Server 2022 | Clean Architecture

---

## Overview

This guide is the single source of truth for the development sequence. Each phase builds on the previous one. Do **not** skip phases — later layers depend on earlier ones being stable.

---

## Phase 1 — Solution & Project Scaffolding

**Goal:** Create the .NET solution structure matching the Clean Architecture design.

### Steps

1. **Create solution and projects**
   ```bash
   dotnet new sln -n StockScreener
   dotnet new classlib -n StockScreener.Domain           -f net10.0
   dotnet new classlib -n StockScreener.Application      -f net10.0
   dotnet new classlib -n StockScreener.Infrastructure   -f net10.0
   dotnet new webapi   -n StockScreener.API              -f net10.0
   dotnet new blazorwasm -n StockScreener.Presentation   -f net10.0
   dotnet new xunit    -n StockScreener.Tests            -f net10.0
   ```

2. **Add projects to solution**
   ```bash
   dotnet sln add StockScreener.Domain/StockScreener.Domain.csproj
   dotnet sln add StockScreener.Application/StockScreener.Application.csproj
   dotnet sln add StockScreener.Infrastructure/StockScreener.Infrastructure.csproj
   dotnet sln add StockScreener.API/StockScreener.API.csproj
   dotnet sln add StockScreener.Presentation/StockScreener.Presentation.csproj
   dotnet sln add StockScreener.Tests/StockScreener.Tests.csproj
   ```

3. **Wire up project references (dependency rules)**
   ```bash
   # Application depends on Domain
   dotnet add StockScreener.Application reference StockScreener.Domain

   # Infrastructure depends on Domain + Application
   dotnet add StockScreener.Infrastructure reference StockScreener.Domain
   dotnet add StockScreener.Infrastructure reference StockScreener.Application

   # API depends on Application + Infrastructure (composition root)
   dotnet add StockScreener.API reference StockScreener.Application
   dotnet add StockScreener.API reference StockScreener.Infrastructure

   # Presentation depends on Application (DTOs only, via API calls)
   # No direct project ref — communicates via HTTP/SignalR

   # Tests reference all layers
   dotnet add StockScreener.Tests reference StockScreener.Domain
   dotnet add StockScreener.Tests reference StockScreener.Application
   dotnet add StockScreener.Tests reference StockScreener.Infrastructure
   ```

4. **Install NuGet packages**

   | Project        | Package                                                             |
   | -------------- | ------------------------------------------------------------------- |
   | Infrastructure | `Microsoft.EntityFrameworkCore.SqlServer`                           |
   | Infrastructure | `Microsoft.EntityFrameworkCore.Tools`                               |
   | Infrastructure | `AutoMapper`                                                        |
   | Infrastructure | `Microsoft.Extensions.Http`                                         |
   | Infrastructure | `Microsoft.AspNetCore.SignalR.Client`                               |
   | API            | `Microsoft.AspNetCore.Authentication.JwtBearer`                     |
   | API            | `Swashbuckle.AspNetCore` (OpenAPI)                                  |
   | Presentation   | `Syncfusion.Blazor.Grid`                                            |
   | Presentation   | `Syncfusion.Blazor.Charts`                                          |
   | Presentation   | `Microsoft.AspNetCore.SignalR.Client`                               |
   | Tests          | `Moq`, `FluentAssertions`, `Microsoft.EntityFrameworkCore.InMemory` |

5. **Verify the build**
   ```bash
   dotnet build
   ```

---

## Phase 2 — Domain Layer

**Goal:** Define all entities, value objects, enums, and repository/service interfaces. Zero external dependencies.

### Steps

1. **Enums** (`StockScreener.Domain/Enums/`)
   - `Exchange.cs` — NYSE, NASDAQ, AMEX, OTC
   - `MarketCapCategory.cs` — Micro, Small, Mid, Large, Mega
   - `SortOrder.cs` — Ascending, Descending
   - `IndicatorType.cs` — RSI, SMA, MACD, BollingerBands, ATR

2. **Value Objects** (`StockScreener.Domain/ValueObjects/`)
   - `Money.cs` — immutable decimal + currency, validation (non-negative)
   - `Percentage.cs` — immutable decimal, validation (0-100 for RSI-style)
   - `DateRange.cs` — start/end date with validation (start ≤ end)

3. **Entities** (`StockScreener.Domain/Entities/`)
   - `Stock.cs` — core stock data (Symbol, CompanyName, Exchange, Sector, MarketCap, CurrentPrice)
   - `PriceHistory.cs` — OHLCV per day, FK to Stock
   - `Fundamentals.cs` — PE, PB, PS, EPS, DividendYield, Beta, ROE, etc., FK to Stock
   - `TechnicalIndicators.cs` — RSI14, SMA20/50/200, MACD, ATR14, Bollinger Bands, FK to Stock
   - `Watchlist.cs` — named list, owned by user
   - `WatchlistItem.cs` — FK to Watchlist + Stock
   - `FilterPreset.cs` — serialized filter JSON, name, description

4. **Repository Interfaces** (`StockScreener.Domain/Interfaces/Repositories/`)
   - `IStockRepository.cs`
   - `IPriceHistoryRepository.cs`
   - `IFundamentalsRepository.cs`
   - `IWatchlistRepository.cs`

5. **Service Interfaces** (`StockScreener.Domain/Interfaces/Services/`)
   - `IIndicatorCalculator.cs` — RSI, MACD, SMA, ATR, Bollinger Bands
   - `IScreenerEngine.cs` — `Filter(IEnumerable<Stock>, ScreenerFilter)` 
   - `IMarketDataProvider.cs` — port for external data (Finnhub/AlphaVantage)

6. **Domain Services** (`StockScreener.Domain/Services/`)
   - `IndicatorCalculator.cs` — pure math, no I/O
   - `ScreenerEngine.cs` — stateless filter logic (AND all active criteria)

7. **Write unit tests** for `IndicatorCalculator` and `ScreenerEngine`

---

## Phase 3 — Application Layer

**Goal:** Define use-case interfaces and implement them using Domain types. All I/O through interfaces (no concrete classes).

### Steps

1. **DTOs** (`StockScreener.Application/DTOs/`)
   - `StockDto.cs`
   - `PriceHistoryDto.cs`
   - `FundamentalsDto.cs`
   - `ScreenerFilterDto.cs` — mirrors query params from the API contract
   - `ScreenerResultDto.cs` — paged result with `PageInfo`
   - `WatchlistDto.cs`

2. **Application Interfaces** (`StockScreener.Application/Interfaces/`)
   - `IScreenerService.cs`
   - `IStockService.cs`
   - `IMarketDataService.cs`
   - `IWatchlistService.cs`

3. **Application Services** (`StockScreener.Application/Services/`)
   - `ScreenerService.cs` — delegates to `IScreenerEngine` + `IStockRepository`
   - `StockService.cs` — get by symbol, price history
   - `MarketDataService.cs` — orchestrates background sync via `IMarketDataProvider`
   - `WatchlistService.cs` — CRUD for watchlists and items

4. **Write unit tests** for all Application services (mock all repository/domain interfaces)

---

## Phase 4 — Database & Infrastructure Layer

**Goal:** Implement persistence, external API clients, caching, and background services.

### Steps

1. **EF Core DbContext** (`StockScreener.Infrastructure/Persistence/AppDbContext.cs`)
   - `DbSet<Stock>`, `DbSet<PriceHistory>`, `DbSet<Fundamentals>`, `DbSet<TechnicalIndicators>`, `DbSet<Watchlist>`, `DbSet<WatchlistItem>`, `DbSet<FilterPreset>`

2. **Entity Configurations** (`StockScreener.Infrastructure/Persistence/Configurations/`)
   - One `IEntityTypeConfiguration<T>` per entity
   - Define indexes: `IX_Stocks_Symbol`, `IX_PriceHistory_StockId_Date`, `IX_TechnicalIndicators_StockId_Date`, `IX_Fundamentals_StockId`
   - Enforce unique constraint on `Stocks.Symbol`

3. **Repository Implementations** (`StockScreener.Infrastructure/Persistence/Repositories/`)
   - `StockRepository.cs` — implements `IStockRepository`
   - `PriceHistoryRepository.cs`
   - `FundamentalsRepository.cs`
   - `WatchlistRepository.cs`

4. **AutoMapper Profile** (`StockScreener.Infrastructure/Mapping/MappingProfile.cs`)
   - Entity → DTO mappings for all types

5. **External API Clients** (`StockScreener.Infrastructure/External/`)
   - `FinnhubClient.cs` — implements `IMarketDataProvider`, typed `HttpClient`
   - `AlphaVantageClient.cs` — fallback provider
   - Response models: `FinnhubQuoteResponse.cs`, `FinnhubCandleResponse.cs`

6. **Caching** (`StockScreener.Infrastructure/Services/CachingService.cs`)
   - Wrap `IMemoryCache`, define cache keys and TTLs

7. **Background Services** (`StockScreener.Infrastructure/Services/`)
   - `BackgroundDataSyncService.cs` — `IHostedService`, syncs daily price + fundamental data from Finnhub
   - `SignalRMarketDataService.cs` — broadcasts real-time price updates

8. **EF Core Migrations**
   ```bash
   dotnet ef migrations add InitialCreate --project StockScreener.Infrastructure --startup-project StockScreener.API
   dotnet ef database update --project StockScreener.Infrastructure --startup-project StockScreener.API
   ```

9. **Write integration tests** for repositories using `InMemory` provider

---

## Phase 5 — API Layer

**Goal:** Expose all functionality via REST endpoints and a SignalR hub, with JWT auth and OpenAPI docs.

### Steps

1. **Configure `Program.cs`**
   - Register all services (DI composition root)
   - Add JWT Bearer authentication
   - Add CORS policy for Blazor WASM origin
   - Add OpenAPI / Swagger
   - Map SignalR hub route

2. **Controllers** (`StockScreener.API/Controllers/`)

   | Controller             | Endpoints                                                                                                                                                       |
   | ---------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
   | `ScreenerController`   | `GET /api/screener/filter`, `GET /api/screener/presets`, `POST /api/screener/presets`, `DELETE /api/screener/presets/{id}`                                      |
   | `StocksController`     | `GET /api/stocks/{symbol}`, `GET /api/stocks/{symbol}/price-history`, `GET /api/stocks/{symbol}/fundamentals`, `GET /api/stocks/{symbol}/indicators`            |
   | `WatchlistController`  | `GET /api/watchlists`, `POST /api/watchlists`, `DELETE /api/watchlists/{id}`, `POST /api/watchlists/{id}/stocks`, `DELETE /api/watchlists/{id}/stocks/{symbol}` |
   | `MarketDataController` | `POST /api/market-data/sync` (manual trigger)                                                                                                                   |

3. **SignalR Hub** (`StockScreener.API/Hubs/MarketDataHub.cs`)
   - Method: `SubscribeToSymbols(string[] symbols)`
   - Broadcast: `PriceUpdate` event with symbol + price + change

4. **Global error handling middleware**
   - Map domain exceptions to correct HTTP status codes (404, 400, 500)

5. **Validate the full API surface with Swagger UI**

---

## Phase 6 — Blazor Presentation Layer

**Goal:** Build the UI — screener page, stock detail, watchlist, and real-time updates.

### Steps

1. **Configure `Program.cs`** (Blazor WASM)
   - Register `HttpClient` pointing at the API base URL
   - Register `SignalRService`
   - Register Syncfusion license + components

2. **Services** (`StockScreener.Presentation/Services/`)
   - `ApiClient.cs` — typed HTTP client wrapping all API endpoints
   - `SignalRService.cs` — connect/disconnect, subscribe to symbols, expose `IObservable<PriceUpdate>`
   - `LocalStorageService.cs` — persist filter presets locally

3. **ViewModels** (`StockScreener.Presentation/ViewModels/`)
   - `ScreenerViewModel.cs` — holds filter state, results, pagination, sort
   - `StockDetailViewModel.cs` — stock data + chart data
   - `WatchlistViewModel.cs` — watchlist CRUD state

4. **Pages** (`StockScreener.Presentation/Pages/`)
   - `Screener.razor` — main screener page, binds `ScreenerViewModel`
   - `StockDetail.razor` — `/stocks/{symbol}` route
   - `Watchlist.razor`
   - `Settings.razor`

5. **Shared Components** (`StockScreener.Presentation/Components/`)

   | Folder     | Components                                                                                                          |
   | ---------- | ------------------------------------------------------------------------------------------------------------------- |
   | `Filters/` | `FilterPanel.razor`, `PriceFilter.razor`, `MarketCapFilter.razor`, `TechnicalFilter.razor`, `ValuationFilter.razor` |
   | `Grid/`    | `StockGrid.razor` (Syncfusion SfGrid), `StockGridRow.razor`                                                         |
   | `Charts/`  | `CandlestickChart.razor` (Syncfusion SfChart), `PriceLineChart.razor`                                               |
   | `Common/`  | `PriceDisplay.razor` (green/red coloring), `LoadingSpinner.razor`, `ConnectionStatus.razor`                         |

6. **Real-time integration**
   - `Screener.razor` subscribes to `SignalRService` on load
   - `PriceDisplay.razor` animates on price change
   - `ConnectionStatus.razor` shows hub connectivity

7. **Routing** (`App.razor`)
   - Define `<Router>` with routes: `/`, `/stocks/{symbol}`, `/watchlist`, `/settings`

---

## Phase 7 — Authentication

**Goal:** Secure all API endpoints and the Blazor app with JWT.

### Steps

1. Add user registration + login endpoints (`AuthController`)
2. Generate and validate JWT tokens (`JwtService` in Infrastructure)
3. Protect all controllers with `[Authorize]`
4. Add `AuthenticationStateProvider` in Blazor WASM
5. Implement login/register pages
6. Store token in `localStorage` (via `LocalStorageService`)
7. Attach `Authorization: Bearer <token>` header in `ApiClient.cs`

---

## Phase 8 — Testing & Quality

**Goal:** Achieve meaningful test coverage on business-critical paths.

### Test Targets

| Area                                   | Type                                | Priority |
| -------------------------------------- | ----------------------------------- | -------- |
| `IndicatorCalculator` — RSI, MACD, SMA | Unit                                | P0       |
| `ScreenerEngine` — filter combinations | Unit                                | P0       |
| `ScreenerService`                      | Unit (mocked repos)                 | P0       |
| `StockService`                         | Unit                                | P1       |
| `StockRepository`                      | Integration (InMemory EF)           | P1       |
| Screener API endpoint                  | Integration (WebApplicationFactory) | P1       |
| Screener UI filtering flow             | E2E (Playwright, optional)          | P2       |

### Checklist
- [ ] All P0 unit tests passing
- [ ] All P1 integration tests passing
- [ ] No unhandled exceptions in API logs during smoke test
- [ ] Swagger UI shows all endpoints with correct schemas

---

## Phase 9 — Data Seeding & External Integration

**Goal:** Connect to Finnhub, load real stock data, verify end-to-end flow.

### Steps

1. Obtain Finnhub API key (free tier: 60 calls/min)
2. Configure key in `appsettings.json` / user secrets
3. Implement `FinnhubClient` quote + candle endpoints
4. Run `BackgroundDataSyncService` manually to seed initial data
5. Verify data appears in the screener
6. Test SignalR real-time updates with live price data
7. Implement AlphaVantage fallback with circuit-breaker logic

---

## Phase 10 — Performance & Hardening

**Goal:** Meet the PRD requirements (3s query response, 5000+ stocks, 99.5% uptime target).

### Steps

1. Add index hints and query optimization to repositories using EF `AsNoTracking()`
2. Cache screener results with 10-second TTL
3. Add pagination enforcement (max page size 200)
4. Add request rate limiting (middleware) for screener endpoint
5. Add health check endpoints (`/health`, `/health/ready`)
6. Load test screener with 5000 stocks (verify < 3s)
7. Profile and fix N+1 query issues with `Include()` / `Split queries`

---

## Development Order Summary

```
Phase 1  → Solution structure & dependencies
Phase 2  → Domain (entities, interfaces, pure logic)
Phase 3  → Application (use cases, DTOs)
Phase 4  → Infrastructure (EF Core, Finnhub, caching, background jobs)
Phase 5  → API (REST controllers, SignalR hub, JWT config)
Phase 6  → Blazor UI (pages, components, ViewModels)
Phase 7  → Authentication (JWT end-to-end)
Phase 8  → Tests (unit + integration)
Phase 9  → Real data (Finnhub integration, seeding)
Phase 10 → Performance & hardening
```

---

## Key Conventions

| Convention                       | Rule                                                                                         |
| -------------------------------- | -------------------------------------------------------------------------------------------- |
| **Dependency direction**         | Domain ← Application ← Infrastructure → API                                                  |
| **No framework refs in Domain**  | Domain is pure C# — no EF, no ASP.NET                                                        |
| **Use DTOs at boundaries**       | Controllers and Blazor pages never use Domain entities directly                              |
| **Primary Constructors**         | Use for all Application services (C# 12+)                                                    |
| **Nullable reference types**     | Enabled in all projects (`<Nullable>enable</Nullable>`)                                      |
| **Async all the way**            | All I/O methods are `async Task<T>` — no `.Result` or `.Wait()`                              |
| **Migrations in Infrastructure** | EF migrations live in `StockScreener.Infrastructure`, startup project is `StockScreener.API` |
