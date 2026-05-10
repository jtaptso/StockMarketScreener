# Stock Market Screener - Technical Design Document

**Version:** 1.0  
**Based on:** PRD-StockMarketScreener.md v1.0  
**Date:** January 2025  
**Tech Stack:** .NET 10 (API) | Blazor WebAssembly (UI) | SQL Server 2022

---

## Table of Contents

1. [System Overview](#1-system-overview)
2. [Architecture Design](#2-architecture-design)
3. [API Contract Specifications](#3-api-contract-specifications)
4. [Database Design](#4-database-design)
5. [Component Design](#5-component-design)
6. [Key Workflows & Sequence Diagrams](#6-key-workflows--sequence-diagrams)
7. [Error Handling Strategy](#7-error-handling-strategy)
8. [Security Design](#8-security-design)
9. [Configuration Management](#9-configuration-management)
10. [Deployment Architecture](#10-deployment-architecture)

---

## 1. System Overview

### 1.1 Purpose

This Design Document provides detailed technical specifications for implementing the Stock Market Screener application. It complements the PRD by specifying exact API contracts, database schemas, component interfaces, and system interactions.

### 1.2 System Context

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              EXTERNAL SYSTEMS                               │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────────┐   │
│  │   Finnhub API   │     │ Alpha Vantage   │     │   Market Exchange   │   │
│  │   (Primary)     │     │    (Backup)     │     │    (Data Source)    │   │
│  └────────┬────────┘     └────────┬────────┘     └──────────┬──────────┘   │
│           │                       │                        │               │
└───────────┼───────────────────────┼────────────────────────┼───────────────┘
            │                       │                        │
            ▼                       ▼                        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         BOPSRSE APPLICATION                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                      Blazor WebAssembly Client                       │   │
│  │   ┌───────────┐  ┌───────────┐  ┌───────────┐  ┌─────────────────┐  │   │
│  │   │ Screener  │  │  Stock    │  │ Watchlist │  │  Settings       │  │   │
│  │   │   Page    │  │  Detail   │  │   Page    │  │    Page         │  │   │
│  │   └─────┬─────┘  └─────┬─────┘  └─────┬─────┘  └────────┬────────┘  │   │
│  │         │              │              │                 │            │   │
│  │         └──────────────┼──────────────┼─────────────────┘            │   │
│  │                        │              │                               │   │
│  │                  ┌─────┴──────────────┴─────┐                        │   │
│  │                  │    State Management      │                        │   │
│  │                  │  (ScreenerViewModel)     │                        │   │
│  │                  └────────────┬─────────────┘                        │   │
│  └───────────────────────────────┼─────────────────────────────────────┘   │
│                                  │ SignalR + HTTP                          
│  ┌───────────────────────────────┼─────────────────────────────────────┐   │
│  │                    ASP.NET Core Web API (.NET 10)                    │   │
│  │   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │   │
│  │   │ Screener     │  │ Stock        │  │ Watchlist    │              │   │
│  │   │ Controller   │  │ Controller   │  │ Controller   │              │   │
│  │   └──────┬───────┘  └──────┬───────┘  └──────┬───────┘              │   │
│  │          │                 │                 │                      │   │
│  │   ┌──────┴─────────────────┴─────────────────┴───────┐              │   │
│  │   │              Application Services Layer           │              │   │
│  │   │  ScreenerService │ StockService │ WatchlistService│              │   │
│  │   └──────┬─────────────────┬─────────────────┬───────┘              │   │
│  │          │                 │                 │                      │   │
│  │   ┌──────┴─────────────────┴─────────────────┴───────┐              │   │
│  │   │                Domain Layer                       │              │   │
│  │   │  ScreenerEngine │ IndicatorCalculator │ Entities │              │   │
│  │   └──────┬─────────────────┬─────────────────┬───────┘              │   │
│  │          │                 │                 │                      │   │
│  │   ┌──────┴─────────────────┴─────────────────┴───────┐              │   │
│  │   │             Infrastructure Layer                  │              │   │
│  │   │  EF Core │ FinnhubClient │ SignalRHub │ Caching  │              │   │
│  │   └──────────────────────────────────────────────────┘              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                  │                                          
│  ┌───────────────────────────────┼─────────────────────────────────────┐   │
│  │                        SQL Server 2022                               │   │
│  │   ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │   │
│  │   │   Stocks     │  │  PriceHistory│  │ Fundamentals │              │   │
│  │   └──────────────┘  └──────────────┘  └──────────────┘              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 1.3 Key Technical Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **API Protocol** | REST + SignalR | REST for CRUD, SignalR for real-time |
| **Authentication** | JWT Bearer Tokens | Industry standard, stateless |
| **ORM** | Entity Framework Core 10 | Native .NET 10 integration, migrations |
| **UI Component Library** | Syncfusion Blazor | Comprehensive financial UI components |
| **Caching** | IMemoryCache (in-process) | Simple, fast for MVP; Redis-ready |
| **Background Jobs** | IHostedService | Native .NET background processing |
| **API Documentation** | OpenAPI/Swagger | Built-in .NET 10 support |

---

## 2. Architecture Design

### 2.1 Clean Architecture Layers (Detailed)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PRESENTATION LAYER                                │
│                         Bourse.Presentation (.NET 10)                       │
├─────────────────────────────────────────────────────────────────────────────┤
│  Blazor Pages          UI Components           ViewModels                   │
│  ├─ Screener.razor     ├─ Filters/             ├─ ScreenerViewModel        
│  ├─ StockDetail.razor  │  ├─ FilterPanel.razor ├─ StockDetailViewModel     
│  ├─ Watchlist.razor    │  ├─ PriceFilter.razor ├─ WatchlistViewModel       
│  └─ Settings.razor     │  └─ ...               └─ ...                       │
│                        ├─ Charts/                                             │
│  Services              │  ├─ CandlestickChart.razor                          │
│  ├─ ApiClient.cs       │  └─ PriceLineChart.razor                           │
│  ├─ SignalRService.cs  ├─ Grid/                                               │
│  └─ LocalStorageService│  └─ StockGrid.razor                                │
│                        └─ Common/                                            │
│  Dependency Injection: services.AddScoped<IStockService, StockService>();   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           APPLICATION LAYER                                 │
│                          Bourse.Application (.NET 10)                       │
├─────────────────────────────────────────────────────────────────────────────┤
│  Use Case Interfaces        Use Case Implementations        DTOs             │
│  ├─ IScreenerService        ├─ ScreenerService           ├─ StockDto        
│  ├─ IStockService           ├─ StockService              ├─ ScreenerFilterDto
│  ├─ IWatchlistService       ├─ WatchlistService          ├─ ScreenerResultDto
│  └─ IMarketDataService      └─ MarketDataService         ├─ WatchlistDto     
│                                                                  └─ ...      
│  Primary Constructor Pattern (C# 13):                                        
│  public ScreenerService(IStockRepository repo, IIndicatorCalculator calc)    │
│  {                                                                            
│      _repo = repo; _calc = calc;                                              │
│  }                                                                            
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                             DOMAIN LAYER                                    │
│                            Bourse.Domain (.NET 10)                          │
├─────────────────────────────────────────────────────────────────────────────┤
│  Entities                  Enums                    Value Objects           │
│  ├─ Stock                  ├─ Exchange              ├─ Money                
│  ├─ PriceHistory           ├─ MarketCapCategory     ├─ DateRange            
│  ├─ Fundamentals           ├─ SortOrder             └─ Percentage           
│  ├─ TechnicalIndicators    └─ IndicatorType                                   │
│  ├─ Watchlist                                                                
│  └─ WatchlistItem                                                            
│                                                                              
│  Repository Interfaces          Domain Services                             │
│  ├─ IStockRepository            ├─ ScreenerEngine                          
│  ├─ IPriceHistoryRepository     └─ IndicatorCalculator                     
│  ├─ IFundamentalsRepository                                                  
│  └─ IWatchlistRepository                                                      
│                                                                              
│  NO external dependencies. Pure C# with no framework references.             │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │
┌─────────────────────────────────────────────────────────────────────────────┐
│                         INFRASTRUCTURE LAYER                                │
│                       Bourse.Infrastructure (.NET 10)                       │
├─────────────────────────────────────────────────────────────────────────────┤
│  Persistence                  External Services           Background Jobs   
│  ├─ AppDbContext              ├─ FinnhubClient           ├─ MarketDataSyncService
│  ├─ Repository implementations├─ AlphaVantageClient      └─ IndicatorCalculationService
│  └─ Entity Configurations     └─ IMarketDataProvider                           
│                                                                              
│  Cross-Cutting                  SignalR                          Caching    
│  ├─ MappingProfile (AutoMapper)├─ MarketDataHub                  ├─ CacheKeys
│  └─ ExceptionInterceptor       └─ SignalRMarketDataService        └─ MemoryCacheService
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Project Dependencies (csproj)

```xml
<!-- Bourse.Domain: No dependencies -->
<Project Sdk Microsoft.NET.Sdk>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>

<!-- Bourse.Application: Depends only on Domain -->
<Project Sdk Microsoft.NET.Sdk>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=..\\Domain\">
  </ItemGroup>
</Project>

<!-- Bourse.Infrastructure: Depends on Domain + Application -->
<Project Sdk Microsoft.NET.Sdk>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=..\\Domain\">
    <ProjectReference Include=..\">
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include Microsoft.EntityFrameworkCore />
    <PackageReference Include Microsoft.EntityFrameworkCore.SqlServer />
    <PackageReference Include Microsoft.Extensions.Caching.Memory />
    <PackageReference Include AutoMapper />
  </ItemGroup>
</Project>

<!-- Bourse.Presentation: Depends on Application -->
<Project Sdk Microsoft.NET.Sdk>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <BlazorWebAssemblySDK>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=..\">
    <PackageReference Include Syncfusion.Blazor />
  </ItemGroup>
</Project>
```

### 2.3 Dependency Injection Configuration

```csharp
// Program.cs in Bourse.Presentation (Blazor)
public static IServiceProvider ConfigureServices()
{
    var services = new ServiceCollection();
    
    // Infrastructure
    services.AddScoped<IStockRepository, StockRepository>();
    services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
    services.AddScoped<IWatchlistRepository, WatchlistRepository>();
    services.AddScoped<IMarketDataProvider, FinnhubClient>();
    services.AddScoped<ICacheService, MemoryCacheService>();
    
    // Application Services (with Primary Constructors)
    services.AddScoped<IScreenerService, ScreenerService>();
    services.AddScoped<IStockService, StockService>();
    services.AddScoped<IWatchlistService, WatchlistService>();
    services.AddScoped<IMarketDataService, MarketDataService>();
    
    // Domain Services
    services.AddScoped<IIndicatorCalculator, IndicatorCalculator>();
    services.AddScoped<IScreenerEngine, ScreenerEngine>();
    
    // Background Services
    services.AddHostedService<MarketDataSyncService>();
    services.AddHostedService<IndicatorCalculationService>();
    
    return services.BuildServiceProvider();
}
```

---

## 3. API Contract Specifications

### 3.1 Screener API

#### GET /api/screener/filter

**Description:** Filter stocks based on multiple criteria with pagination and sorting.

**Request Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| minPrice | decimal? | No | null | Minimum stock price |
| maxPrice | decimal? | No | null | Maximum stock price |
| marketCapCategory | string? | No | null | Micro/Small/Mid/Large/Mega |
| sector | string? | No | null | Industry sector |
| industry | string? | No | null | Sub-industry |
| minPE | decimal? | No | null | Minimum P/E ratio |
| maxPE | decimal? | No | null | Maximum P/E ratio |
| minPB | decimal? | No | null | Minimum P/B ratio |
| maxPB | decimal? | No | null | Maximum P/B ratio |
| minRSI | decimal? | No | null | Minimum RSI (0-100) |
| maxRSI | decimal? | No | null | Maximum RSI (0-100) |
| minMarketCap | decimal? | No | null | Minimum market cap in USD |
| maxMarketCap | decimal? | No | null | Maximum market cap in USD |
| minDividendYield | decimal? | No | null | Minimum dividend yield % |
| minVolume | long? | No | null | Minimum average volume |
| aboveSMA50 | bool? | No | null | Price above 50-day SMA |
| belowSMA200 | bool? | No | null | Price below 200-day SMA |
| sortBy | string | No | Symbol | Sort column |
| sortOrder | string | No | Asc | Asc/Desc |
| page | int | No | 1 | Page number |
| pageSize | int | No | 50 | Items per page (max 200) |

**Response (200 OK):**

```json
{
  "items": [
    {
      "symbol": "AAPL",
      "companyName": "Apple Inc.",
      "exchange": "NASDAQ",
      "sector": "Technology",
      "currentPrice": 178.50,
      "marketCap": 2800000000000,
      "marketCapCategory": "Mega",
      "peRatio": 28.5,
      "pbRatio": 45.2,
      "dividendYield": 0.52,
      "rsi14": 65.4,
      "sma50": 175.25,
      "sma200": 165.80,
      "volume": 52000000,
      "priceChange": 2.35,
      "priceChangePercent": 1.33,
      "lastUpdated": "2025-01-15T16:00:00Z"
    }
  ],
  "pageInfo": {
    "currentPage": 1,
    "pageSize": 50,
    "totalItems": 1250,
    "totalPages": 25,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "appliedFilters": {
    "minPrice": null,
    "maxPrice": null,
    "marketCapCategory": "Large",
    "sector": "Technology",
    "minRSI": 30,
    "maxRSI": 70
  }
}
```

**Error Responses:**
- 400 Bad Request: Invalid filter parameters
- 500 Internal Server Error: Server error

---

#### GET /api/screener/presets

**Description:** Get all saved filter presets for the current user.

**Response (200 OK):**

```json
{
  "presets": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Tech Growth Stocks",
      "description": "Large cap tech with strong RSI",
      "filters": {
        "sector": "Technology",
        "marketCapCategory": "Large",
        "minRSI": 40,
        "maxRSI": 70,
        "minRevenueGrowth": 10
      },
      "createdAt": "2025-01-10T09:30:00Z",
      "updatedAt": "2025-01-12T14:20:00Z"
    }
  ]
}
```

---

#### POST /api/screener/presets

**Description:** Create a new filter preset.

**Request Body:**

```json
{
  \"name\": \"High Dividend Yield\",
  \"description\": \"Stocks with dividend yield > 3%\",
  \"filters\": {
    \"minDividendYield\": 3.0,
    \"minMarketCap\": 10000000000,
    \"maxPE\": 25
  }
}
```

**Response (201 Created):**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "High Dividend Yield",
  "description": "Stocks with dividend yield > 3%",
  "filters": {
    "minDividendYield": 3.0,
    "minMarketCap": 10000000000,
    "maxPE": 25
  },
  "createdAt": "2025-01-15T10:00:00Z"
}
```

---

#### DELETE /api/screener/presets/{id}

**Description:** Delete a filter preset.

**Response:** 204 No Content

**Error Responses:**
- 404 Not Found: Preset not found

---

### 3.2 Stock Data API

#### GET /api/stocks/{symbol}

**Description:** Get detailed information for a specific stock.

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| symbol | string | Yes | Stock ticker symbol (e.g., AAPL) |

**Response (200 OK):**

```json
{
  "symbol": "AAPL",
  "companyName": "Apple Inc.",
  "exchange": "NASDAQ",
  "sector": "Technology",
  "industry": "Consumer Electronics",
  "marketCap": 2800000000000,
  "marketCapCategory": "Mega",
  "currentPrice": 178.50,
  "priceChange": 2.35,
  "priceChangePercent": 1.33,
  "dayHigh": 179.80,
  "dayLow": 176.20,
  "week52High": 199.62,
  "week52Low": 124.17,
  "volume": 52000000,
  "avgVolume": 48000000,
  "beta": 1.29,
  "lastUpdated": "2025-01-15T16:00:00Z"
}
```

**Error Responses:**
- 404 Not Found: Stock symbol not found

---

#### GET /api/stocks/{symbol}/price-history

**Description:** Get historical price data for a stock.

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| symbol | string | Yes | Stock ticker symbol |

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| fromDate | date | No | 1 year ago | Start date (ISO 8601) |
| toDate | date | No | today | End date (ISO 8601) |
| interval | string | No | daily | daily/weekly/monthly |

**Response (200 OK):**

```json
{
  "symbol": "AAPL",
  "interval": "daily",
  "data": [
    {
      "date": "2025-01-15",
      "open": 176.50,
      "high": 179.80,
      "low": 176.20,
      "close": 178.50,
      "volume": 52000000
    },
    {
      "date": "2025-01-14",
      "open": 175.00,
      "high": 177.30,
      "low": 174.80,
      "close": 176.15,
      "volume": 45000000
    }
  ]
}
```

---

#### GET /api/stocks/{symbol}/fundamentals

**Description:** Get fundamental data for a stock.

**Response (200 OK):**

```json
{
  "symbol": "AAPL",
  "peRatio": 28.5,
  "pbRatio": 45.2,
  "psRatio": 7.8,
  "eps": 6.26,
  "dividendYield": 0.52,
  "beta": 1.29,
  "debtToEquity": 1.2,
  "currentRatio": 1.5,
  "quickRatio": 1.3,
  "roe": 150.2,
  "revenueGrowth": 8.1,
  "profitMargin": 24.5,
  "operatingMargin": 30.2,
  "grossMargin": 45.0,
  "revenue": 385600000000,
  "netIncome": 97000000000,
  "totalDebt": 125000000000,
  "totalEquity": 740000000000,
  "fiscalYearEnd": "2024-09-30",
  "lastUpdated": "2025-01-15T00:00:00Z"
}
```

---

#### GET /api/stocks/{symbol}/indicators

**Description:** Get technical indicators for a stock.

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| days | int | No | 30 | Number of days of data |

**Response (200 OK):**

```json
{
  "symbol": "AAPL",
  "latest": {
    "rsi14": 65.4,
    "sma20": 177.50,
    "sma50": 175.25,
    "sma200": 165.80,
    "macd": 2.35,
    "macdSignal": 1.85,
    "macdHistogram": 0.50,
    "atr14": 2.85,
    "bbUpper": 181.50,
    "bbMiddle": 177.50,
    "bbLower": 173.50,
    "volume": 52000000,
    "avgVolume20": 48000000
  },
  "history": [
    {
      "date": "2025-01-15",
      "rsi14": 65.4,
      "sma20": 177.50,
      "sma50": 175.25
    }
  ]
}
```

---

#### GET /api/stocks/search

**Description:** Search for stocks by symbol or company name.

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| query | string | Yes | Search term (min 2 chars) |
| limit | int | No | Max results (default 10) |

**Response (200 OK):**

```json
{
  "results": [
    {
      "symbol": "AAPL",
      "companyName": "Apple Inc.",
      "exchange": "NASDAQ",
      "type": "Stock"
    },
    {
      "symbol": "AAPL.MX",
      "companyName": "Apple Inc.",
      "exchange": "Mexico",
      "type": "Stock"
    }
  ]
}
```

---

### 3.3 Market Data API (Real-time)

#### GET /api/market/quotes

**Description:** Get real-time quotes for multiple symbols.

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| symbols | string | Yes | Comma-separated symbols (e.g., AAPL,MSFT,GOOGL) |

**Response (200 OK):**

```json
{
  "quotes": [
    {
      "symbol": "AAPL",
      "price": 178.50,
      "change": 2.35,
      "changePercent": 1.33,
      "dayHigh": 179.80,
      "dayLow": 176.20,
      "volume": 52000000,
      "timestamp": "2025-01-15T16:00:00Z"
    },
    {
      "symbol": "MSFT",
      "price": 405.20,
      "change": -1.80,
      "changePercent": -0.44,
      "dayHigh": 408.50,
      "dayLow": 403.10,
      "volume": 21000000,
      "timestamp": "2025-01-15T16:00:00Z"
    }
  ]
}
```

---

#### GET /api/market/status

**Description:** Get current market status.

**Response (200 OK):**

```json
{
  "marketStatus": "Open",
  "nextOpen": "2025-01-16T09:30:00-05:00",
  "nextClose": "2025-01-15T16:00:00-05:00",
  "serverTime": "2025-01-15T15:45:00Z",
  "tradingDay": "2025-01-15"
}
```

---

### 3.4 Watchlist API

#### GET /api/watchlists

**Description:** Get all watchlists for the current user.

**Response (200 OK):**

```json
{
  "watchlists": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Tech Favorites",
      "description": "My top tech stock picks",
      "itemCount": 15,
      "createdAt": "2025-01-05T10:00:00Z",
      "updatedAt": "2025-01-15T14:30:00Z"
    }
  ]
}
```

---

#### POST /api/watchlists

**Description:** Create a new watchlist.

**Request Body:**

```json
{
  \"name\": \"Dividend Stocks\",
  \"description\": \"High yield dividend stocks\"
}
```

**Response (201 Created):**

```json
{
  "id": "8e7f9a2b-1234-5678-9abc-def012345678",
  "name": "Dividend Stocks",
  "description": "High yield dividend stocks",
  "itemCount": 0,
  "createdAt": "2025-01-15T16:00:00Z"
}
```

---

#### GET /api/watchlists/{id}

**Description:** Get a specific watchlist with all items.

**Response (200 OK):**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Tech Favorites",
  "description": "My top tech stock picks",
  "items": [
    {
      "stockSymbol": "AAPL",
      "companyName": "Apple Inc.",
      "addedAt": "2025-01-05T10:00:00Z",
      "sharesOwned": 50,
      "costBasis": 8500.00,
      "currentPrice": 178.50,
      "totalValue": 8925.00,
      "gainLoss": 425.00,
      "gainLossPercent": 5.0
    },
    {
      "stockSymbol": "MSFT",
      "companyName": "Microsoft Corporation",
      "addedAt": "2025-01-06T11:00:00Z",
      "sharesOwned": 25,
      "costBasis": 9500.00,
      "currentPrice": 405.20,
      "totalValue": 10130.00,
      "gainLoss": 630.00,
      "gainLossPercent": 6.63
    }
  ],
  "totalValue": 19055.00,
  "totalGainLoss": 1055.00,
  "totalGainLossPercent": 5.85
}
```

---

#### PUT /api/watchlists/{id}

**Description:** Update a watchlist.

**Request Body:**

```json
{
  \"name\": \"Tech Favorites - Updated\",
  \"description\": \"Updated description\"
}
```

**Response (200 OK):** Returns updated watchlist.

---

#### DELETE /api/watchlists/{id}

**Description:** Delete a watchlist and all its items.

**Response:** 204 No Content

---

#### POST /api/watchlists/{id}/items

**Description:** Add a stock to a watchlist.

**Request Body:**

```json
{
  \"stockSymbol\": \"GOOGL\",
  \"sharesOwned\": 10,
  \"costBasis\": 1500.00
}
```

**Response (201 Created):**

```json
{
  "stockSymbol": "GOOGL",
  "companyName": "Alphabet Inc.",
  "addedAt": "2025-01-15T17:00:00Z",
  "sharesOwned": 10,
  "costBasis": 1500.00
}
```

---

#### DELETE /api/watchlists/{id}/items/{symbol}

**Description:** Remove a stock from a watchlist.

**Response:** 204 No Content

---

### 3.5 SignalR Hub Contract

**Hub Path:** `/hubs/market-data`

#### Client → Server Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| SubscribeToSymbols | string[] symbols | Subscribe to price updates for symbols |
| UnsubscribeFromSymbols | string[] symbols | Unsubscribe from symbols |
| GetConnectionState | - | Get current connection state |

#### Server → Client Events

| Event | Payload | Description |
|-------|---------|-------------|
| OnPriceUpdate | `{symbol, price, change, changePercent, timestamp}` | Real-time price update |
| OnMarketStatusChange | `{status, nextOpen, nextClose}` | Market status change |
| OnConnectionStateChanged | `{state, reconnectedAt}` | Connection state change |
| OnBatchPriceUpdate | `PriceUpdate[]` | Batch of price updates (for initial load) |

#### Connection State Enum

```csharp
public enum HubConnectionState
{
    Disconnected = 0,
    Connecting = 1,
    Connected = 2,
    Reconnecting = 3,
    Error = 4
}
```

#### JavaScript Client Example

```javascript
// Connection setup
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/market-data')
    .withAutomaticReconnect()
    .build();

// Register handlers
connection.on('OnPriceUpdate', (update) => {
    console.log(`${update.symbol}: $${update.price}`);
    updateStockPrice(update.symbol, update.price, update.changePercent);
});

connection.on('OnMarketStatusChange', (status) => {
    updateMarketStatus(status.marketStatus);
});

// Start connection
await connection.start();

// Subscribe to symbols
await connection.invoke('SubscribeToSymbols', ['AAPL', 'MSFT', 'GOOGL']);

// Unsubscribe
await connection.invoke('UnsubscribeFromSymbols', ['GOOGL']);
```

---

## 4. Database Design

### 4.1 Database Schema (DDL)

```sql
-- Create Database
CREATE DATABASE BourseDb
COLLATE Latin1_General_CI_AI;
GO

-- Use Database
USE BourseDb;
GO

-- ============================================
-- TABLE: Stocks (Master Stock Table)
-- ============================================
CREATE TABLE Stocks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Symbol NVARCHAR(10) NOT NULL,
    CompanyName NVARCHAR(200) NOT NULL,
    Exchange NVARCHAR(50) NOT NULL,
    Sector NVARCHAR(100) NULL,
    Industry NVARCHAR(100) NULL,
    MarketCap DECIMAL(18,2) NULL,
    CurrentPrice DECIMAL(18,4) NULL,
    DayHigh DECIMAL(18,4) NULL,
    DayLow DECIMAL(18,4) NULL,
    Week52High DECIMAL(18,4) NULL,
    Week52Low DECIMAL(18,4) NULL,
    Volume BIGINT NULL,
    AvgVolume BIGINT NULL,
    Beta DECIMAL(10,4) NULL,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT UQ_Stocks_Symbol UNIQUE (Symbol),
    CONSTRAINT CK_Stocks_CurrentPrice CHECK (CurrentPrice >= 0),
    CONSTRAINT CK_Stocks_MarketCap CHECK (MarketCap >= 0)
);

CREATE INDEX IX_Stocks_Symbol ON Stocks(Symbol);
CREATE INDEX IX_Stocks_Sector ON Stocks(Sector);
CREATE INDEX IX_Stocks_Exchange ON Stocks(Exchange);
CREATE INDEX IX_Stocks_MarketCap ON Stocks(MarketCap);

-- ============================================
-- TABLE: PriceHistory
-- ============================================
CREATE TABLE PriceHistory (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    StockId INT NOT NULL,
    TradeDate DATE NOT NULL,
    OpenPrice DECIMAL(18,4) NOT NULL,
    HighPrice DECIMAL(18,4) NOT NULL,
    LowPrice DECIMAL(18,4) NOT NULL,
    ClosePrice DECIMAL(18,4) NOT NULL,
    Volume BIGINT NOT NULL,
    
    CONSTRAINT FK_PriceHistory_Stock 
        FOREIGN KEY (StockId) REFERENCES Stocks(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_PriceHistory_Stock_Date 
        UNIQUE (StockId, TradeDate),
    CONSTRAINT CK_PriceHistory_Prices 
        CHECK (OpenPrice >= 0 AND HighPrice >= 0 AND LowPrice >= 0 AND ClosePrice >= 0),
    CONSTRAINT CK_PriceHistory_HighLow 
        CHECK (HighPrice >= LowPrice)
);

CREATE INDEX IX_PriceHistory_StockId_Date 
    ON PriceHistory(StockId, TradeDate DESC);
CREATE INDEX IX_PriceHistory_Date 
    ON PriceHistory(TradeDate);

-- ============================================
-- TABLE: Fundamentals
-- ============================================
CREATE TABLE Fundamentals (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    StockId INT NOT NULL,
    
    -- Valuation Metrics
    PE_Ratio DECIMAL(18,4) NULL,
    PB_Ratio DECIMAL(18,4) NULL,
    PS_Ratio DECIMAL(18,4) NULL,
    EPS DECIMAL(18,4) NULL,
    
    -- Dividend
    DividendYield DECIMAL(10,4) NULL,
    ExDividendDate DATE NULL,
    
    -- Financial Health
    DebtToEquity DECIMAL(18,4) NULL,
    CurrentRatio DECIMAL(10,4) NULL,
    QuickRatio DECIMAL(10,4) NULL,
    
    -- Profitability
    ROE DECIMAL(10,4) NULL,
    ProfitMargin DECIMAL(10,4) NULL,
    OperatingMargin DECIMAL(10,4) NULL,
    GrossMargin DECIMAL(10,4) NULL,
    
    -- Growth
    RevenueGrowth DECIMAL(10,4) NULL,
    EPSGrowth DECIMAL(10,4) NULL,
    
    -- Income Statement
    Revenue DECIMAL(18,2) NULL,
    NetIncome DECIMAL(18,2) NULL,
    TotalDebt DECIMAL(18,2) NULL,
    TotalEquity DECIMAL(18,2) NULL,
    
    -- Metadata
    FiscalYearEnd DATE NULL,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Fundamentals_Stock 
        FOREIGN KEY (StockId) REFERENCES Stocks(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Fundamentals_StockId UNIQUE (StockId),
    CONSTRAINT CK_Fundamentals_Ratios CHECK (PE_Ratio >= 0 AND PB_Ratio >= 0 AND PS_Ratio >= 0)
);

CREATE INDEX IX_Fundamentals_StockId ON Fundamentals(StockId);
CREATE INDEX IX_Fundamentals_PE ON Fundamentals(PE_Ratio) 
    WHERE PE_Ratio IS NOT NULL;
CREATE INDEX IX_Fundamentals_ROE ON Fundamentals(ROE) 
    WHERE ROE IS NOT NULL;

-- ============================================
-- TABLE: TechnicalIndicators
-- ============================================
CREATE TABLE TechnicalIndicators (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    StockId INT NOT NULL,
    TradeDate DATE NOT NULL,
    
    -- Momentum
    RSI_14 DECIMAL(10,4) NULL,
    
    -- Moving Averages
    SMA_20 DECIMAL(18,4) NULL,
    SMA_50 DECIMAL(18,4) NULL,
    SMA_200 DECIMAL(18,4) NULL,
    
    -- MACD
    MACD DECIMAL(18,4) NULL,
    MACD_Signal DECIMAL(18,4) NULL,
    MACD_Histogram DECIMAL(18,4) NULL,
    
    -- Volatility
    ATR_14 DECIMAL(18,4) NULL,
    BB_Upper DECIMAL(18,4) NULL,
    BB_Middle DECIMAL(18,4) NULL,
    BB_Lower DECIMAL(18,4) NULL,
    
    -- Volume
    Volume BIGINT NULL,
    AvgVolume_20 BIGINT NULL,
    
    -- Metadata
    LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_TechnicalIndicators_Stock 
        FOREIGN KEY (StockId) REFERENCES Stocks(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TechnicalIndicators_Stock_Date 
        UNIQUE (StockId, TradeDate),
    CONSTRAINT CK_TechnicalIndicators_RSI CHECK (RSI_14 IS NULL OR (RSI_14 >= 0 AND RSI_14 <= 100))
);

CREATE INDEX IX_TechnicalIndicators_StockId_Date 
    ON TechnicalIndicators(StockId, TradeDate DESC);
CREATE INDEX IX_TechnicalIndicators_RSI 
    ON TechnicalIndicators(RSI_14) 
    WHERE RSI_14 IS NOT NULL;

-- ============================================
-- TABLE: Watchlists
-- ============================================
CREATE TABLE Watchlists (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    UserId NVARCHAR(450) NOT NULL,  -- ASP.NET Identity User ID
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT CK_Watchlists_Name CHECK (LEN(Name) > 0)
);

CREATE INDEX IX_Watchlists_UserId ON Watchlists(UserId);

-- ============================================
-- TABLE: WatchlistItems
-- ============================================
CREATE TABLE WatchlistItems (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    WatchlistId UNIQUEIDENTIFIER NOT NULL,
    StockId INT NOT NULL,
    SharesOwned DECIMAL(18,4) NULL,
    CostBasis DECIMAL(18,2) NULL,
    AddedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_WatchlistItems_Watchlist 
        FOREIGN KEY (WatchlistId) REFERENCES Watchlists(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WatchlistItems_Stock 
        FOREIGN KEY (StockId) REFERENCES Stocks(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_WatchlistItems_Watchlist_Stock 
        UNIQUE (WatchlistId, StockId)
);

CREATE INDEX IX_WatchlistItems_WatchlistId ON WatchlistItems(WatchlistId);
CREATE INDEX IX_WatchlistItems_StockId ON WatchlistItems(StockId);

-- ============================================
-- TABLE: FilterPresets
-- ============================================
CREATE TABLE FilterPresets (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    FilterJson NVARCHAR(MAX) NOT NULL,  -- JSON serialization of filter criteria
    UserId NVARCHAR(450) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT CK_FilterPresets_Name CHECK (LEN(Name) > 0)
);

CREATE INDEX IX_FilterPresets_UserId ON FilterPresets(UserId);

-- ============================================
-- TABLE: MarketStatus
-- ============================================
CREATE TABLE MarketStatus (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IsOpen BIT NOT NULL,
    MarketName NVARCHAR(50) NOT NULL,
    OpenTime TIME NOT NULL,
    CloseTime TIME NOT NULL,
    TimeZone NVARCHAR(50) NOT NULL,
    LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

INSERT INTO MarketStatus (MarketName, IsOpen, OpenTime, CloseTime, TimeZone)
VALUES ('NYSE/NASDAQ', 0, '09:30:00', '16:00:00', 'America/New_York');
GO
```

### 4.2 Entity Framework Core Configuration

```csharp
// Bourse.Infrastructure/Persistence/Configurations/StockEntityConfiguration.cs
public class StockEntityConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.ToTable(nameof(Stock));
        
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Symbol)
            .IsRequired()
            .HasMaxLength(10);
        builder.HasIndex(s => s.Symbol).IsUnique();
        
        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(s => s.Exchange)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(s => s.Sector)
            .HasMaxLength(100);
        
        builder.Property(s => s.Industry)
            .HasMaxLength(100);
        
        builder.Property(s => s.MarketCap)
            .HasPrecision(18, 2);
        
        builder.Property(s => s.CurrentPrice)
            .HasPrecision(18, 4);
        
        builder.Property(s => s.Beta)
            .HasPrecision(10, 4);
        
        // Relationships
        builder.HasOne(s => s.Fundamentals)
            .WithOne(f => f.Stock)
            .HasForeignKey<Fundamentals>(f => f.StockId);
        
        builder.HasMany(s => s.PriceHistory)
            .WithOne(ph => ph.Stock)
            .HasForeignKey(ph => ph.StockId);
        
        builder.HasMany(s => s.TechnicalIndicators)
            .WithOne(ti => ti.Stock)
            .HasForeignKey(ti => ti.StockId);
    }
}
```

### 4.3 Database Migration Strategy

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate --project src/Bourse.Infrastructure --startup-project src/Bourse.Presentation

# Apply migration to database
dotnet ef database update --project src/Bourse.Infrastructure --startup-project src/Bourse.Presentation

# Generate SQL script for production deployment
dotnet ef migrations script --project src/Bourse.Infrastructure --output migrations.sql
```

---

## 5. Component Design

### 5.1 Domain Entities

```csharp
// Bourse.Domain/Entities/Stock.cs
public class Stock
{
    public int Id { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public Exchange Exchange { get; init; }
    public string? Sector { get; init; }
    public string? Industry { get; init; }
    public decimal? MarketCap { get; init; }
    public decimal? CurrentPrice { get; set; }
    public decimal? DayHigh { get; set; }
    public decimal? DayLow { get; set; }
    public decimal? Week52High { get; set; }
    public decimal? Week52Low { get; set; }
    public long? Volume { get; set; }
    public long? AvgVolume { get; set; }
    public decimal? Beta { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; init; }
    
    // Navigation properties
    public Fundamentals? Fundamentals { get; init; }
    public ICollection<PriceHistory> PriceHistory { get; init; } = new List<PriceHistory>();
    public ICollection<TechnicalIndicators> TechnicalIndicators { get; init; } = new List<TechnicalIndicators>();
    
    // Computed properties
    public MarketCapCategory MarketCapCategory => MarketCap switch
    {
        < 300_000_000 => MarketCapCategory.Micro,
        < 2_000_000_000 => MarketCapCategory.Small,
        < 10_000_000_000 => MarketCapCategory.Mid,
        < 200_000_000_000 => MarketCapCategory.Large,
        _ => MarketCapCategory.Mega
    };
}
```

```csharp
// Bourse.Domain/Entities/PriceHistory.cs
public class PriceHistory
{
    public long Id { get; init; }
    public int StockId { get; init; }
    public DateOnly TradeDate { get; init; }
    public decimal OpenPrice { get; init; }
    public decimal HighPrice { get; init; }
    public decimal LowPrice { get; init; }
    public decimal ClosePrice { get; init; }
    public long Volume { get; init; }
    
    // Navigation property
    public Stock Stock { get; init; } = null!;
    
    // Computed
    public decimal PriceChange => ClosePrice - OpenPrice;
    public decimal PriceChangePercent => OpenPrice != 0 
        ? (PriceChange / OpenPrice) * 100 
        : 0;
    public decimal Range => HighPrice - LowPrice;
}
```

```csharp
// Bourse.Domain/Entities/Fundamentals.cs
public class Fundamentals
{
    public int Id { get; init; }
    public int StockId { get; init; }
    
    // Valuation
    public decimal? PE_Ratio { get; init; }
    public decimal? PB_Ratio { get; init; }
    public decimal? PS_Ratio { get; init; }
    public decimal? EPS { get; init; }
    
    // Dividends
    public decimal? DividendYield { get; init; }
    public DateOnly? ExDividendDate { get; init; }
    
    // Financial Health
    public decimal? DebtToEquity { get; init; }
    public decimal? CurrentRatio { get; init; }
    public decimal? QuickRatio { get; init; }
    
    // Profitability
    public decimal? ROE { get; init; }
    public decimal? ProfitMargin { get; init; }
    public decimal? OperatingMargin { get; init; }
    public decimal? GrossMargin { get; init; }
    
    // Growth
    public decimal? RevenueGrowth { get; init; }
    public decimal? EPSGrowth { get; init; }
    
    // Financials
    public decimal? Revenue { get; init; }
    public decimal? NetIncome { get; init; }
    public decimal? TotalDebt { get; init; }
    public decimal? TotalEquity { get; init; }
    
    public DateOnly? FiscalYearEnd { get; init; }
    public DateTime LastUpdated { get; init; }
    
    public Stock Stock { get; init; } = null!;
}
```

### 5.2 Domain Services

```csharp
// Bourse.Domain/Services/ScreenerEngine.cs
public class ScreenerEngine
{
    public IEnumerable<Stock> Filter(
        IEnumerable<Stock> stocks,
        ScreenerFilterDto filter)
    {
        var query = stocks.AsQueryable();
        
        // Price filter
        if (filter.MinPrice.HasValue)
            query = query.Where(s => s.CurrentPrice >= filter.MinPrice.Value);
        if (filter.MaxPrice.HasValue)
            query = query.Where(s => s.CurrentPrice <= filter.MaxPrice.Value);
        
        // Market cap filter
        if (filter.MinMarketCap.HasValue)
            query = query.Where(s => s.MarketCap >= filter.MinMarketCap.Value);
        if (filter.MaxMarketCap.HasValue)
            query = query.Where(s => s.MarketCap <= filter.MaxMarketCap.Value);
        
        // Sector filter
        if (!string.IsNullOrEmpty(filter.Sector))
            query = query.Where(s => s.Sector == filter.Sector);
        
        // P/E ratio filter
        if (filter.MinPE.HasValue)
            query = query.Where(s => s.Fundamentals != null 
                && s.Fundamentals.PE_Ratio >= filter.MinPE.Value);
        
        // RSI filter
        if (filter.MinRSI.HasValue || filter.MaxRSI.HasValue)
        {
            query = query.Where(s => s.TechnicalIndicators.Any());
            // Additional RSI filtering in application layer
        }
        
        // Volume filter
        if (filter.MinVolume.HasValue)
            query = query.Where(s => s.AvgVolume >= filter.MinVolume);
        
        return query;
    }
    
    public IEnumerable<Stock> Sort(
        IEnumerable<Stock> stocks,
        string sortBy,
        SortOrder sortOrder)
    {
        return sortBy.ToLowerInvariant() switch
        {
            nameof(Stock.CurrentPrice) => sortOrder == SortOrder.Asc
                ? stocks.OrderBy(s => s.CurrentPrice)
                : stocks.OrderByDescending(s => s.CurrentPrice),
            nameof(Stock.MarketCap) => sortOrder == SortOrder.Asc
                ? stocks.OrderBy(s => s.MarketCap)
                : stocks.OrderByDescending(s => s.MarketCap),
            _ => sortOrder == SortOrder.Asc
                ? stocks.OrderBy(s => s.Symbol)
                : stocks.OrderByDescending(s => s.Symbol)
        };
    }
}
```

```csharp
// Bourse.Domain/Services/IndicatorCalculator.cs
public class IndicatorCalculator
{
    // Calculate Simple Moving Average
    public decimal CalculateSMA(IEnumerable<decimal> prices, int period)
    {
        var priceList = prices.TakeLast(period).ToList();
        return priceList.Count == period 
            ? priceList.Average() 
            : 0;
    }
    
    // Calculate RSI (Relative Strength Index)
    public decimal CalculateRSI(IEnumerable<PriceHistory> priceHistory, int period = 14)
    {
        var closes = priceHistory.Select(p => p.ClosePrice).ToList();
        if (closes.Count < period + 1)
            return 50; // Neutral value
            
        var gains = new List<decimal>();
        var losses = new List<decimal>();
        
        for (int i = 1; i < closes.Count; i++)
        {
            var change = closes[i] - closes[i - 1];
            if (change > 0)
            {
                gains.Add(change);
                losses.Add(0);
            }
            else
            {
                gains.Add(0);
                losses.Add(Math.Abs(change));
            }
        }
        
        var avgGain = gains.TakeLast(period).Average();
        var avgLoss = losses.TakeLast(period).Average();
        
        if (avgLoss == 0)
            return 100;
            
        var rs = avgGain / avgLoss;
        return 100 - (100 / (1 + rs));
    }
    
    // Calculate MACD (Moving Average Convergence Divergence)
    public (decimal macd, decimal signal, decimal histogram) CalculateMACD(
        IEnumerable<decimal> prices,
        int fastPeriod = 12,
        int slowPeriod = 26,
        int signalPeriod = 9)
    {
        var priceList = prices.ToList();
        var emaFast = CalculateEMA(priceList, fastPeriod);
        var emaSlow = CalculateEMA(priceList, slowPeriod);
        
        var macdLine = emaFast - emaSlow;
        
        // Signal line is EMA of MACD line (simplified)
        var macdHistory = new List<decimal> { macdLine };
        var signalLine = CalculateEMA(macdHistory, signalPeriod);
        
        return (macdLine, signalLine, macdLine - signalLine);
    }
    
    // Calculate EMA (Exponential Moving Average)
    public decimal CalculateEMA(IEnumerable<decimal> prices, int period)
    {
        var priceList = prices.ToList();
        if (priceList.Count < period)
            return 0;
            
        var multiplier = 2.0m / (period + 1);
        decimal ema = priceList.Take(period).Average();
        
        for (int i = period; i < priceList.Count; i++)
        {
            ema = (priceList[i] - ema) * multiplier + ema;
        }
        
        return ema;
    }
    
    // Calculate ATR (Average True Range)
    public decimal CalculateATR(IEnumerable<PriceHistory> priceHistory, int period = 14)
    {
        var history = priceHistory.TakeLast(period + 1).ToList();
        if (history.Count < period + 1)
            return 0;
            
        var trueRanges = new List<decimal>();
        
        for (int i = 1; i < history.Count; i++)
        {
            var highLow = history[i].HighPrice - history[i].LowPrice;
            var highClose = Math.Abs(history[i].HighPrice - history[i - 1].ClosePrice);
            var lowClose = Math.Abs(history[i].LowPrice - history[i - 1].ClosePrice);
            
            trueRanges.Add(Math.Max(highLow, Math.Max(highClose, lowClose)));
        }
        
        return trueRanges.Average();
    }
    
    // Calculate Bollinger Bands
    public (decimal upper, decimal middle, decimal lower) CalculateBollingerBands(
        IEnumerable<decimal> prices,
        int period = 20,
        decimal standardDeviations = 2)
    {
        var priceList = prices.TakeLast(period).ToList();
        if (priceList.Count < period)
            return (0, 0, 0);
            
        var sma = priceList.Average();
        var stdDev = CalculateStandardDeviation(priceList, sma);
        
        return (
            sma + (standardDeviations * stdDev),
            sma,
            sma - (standardDeviations * stdDev)
        );
    }
    
    private decimal CalculateStandardDeviation(List<decimal> values, decimal mean)
    {
        var sumOfSquares = values.Sum(v => (v - mean) * (v - mean));
        var variance = sumOfSquares / values.Count;
        return (decimal)Math.Sqrt((double)variance);
    }
}
```

### 5.3 Application Service Interfaces

```csharp
// Bourse.Application/Interfaces/IScreenerService.cs
public interface IScreenerService
{
    Task<ScreenerResultDto> FilterStocksAsync(ScreenerFilterDto filter, CancellationToken ct = default);
    Task<IEnumerable<FilterPresetDto>> GetPresetsAsync(string userId, CancellationToken ct = default);
    Task<FilterPresetDto> CreatePresetAsync(CreatePresetDto preset, string userId, CancellationToken ct = default);
    Task DeletePresetAsync(Guid presetId, string userId, CancellationToken ct = default);
}
```

```csharp
// Bourse.Application/Interfaces/IStockService.cs
public interface IStockService
{
    Task<StockDto?> GetStockAsync(string symbol, CancellationToken ct = default);
    Task<PriceHistoryDto> GetPriceHistoryAsync(string symbol, DateOnly fromDate, DateOnly toDate, string interval, CancellationToken ct = default);
    Task<FundamentalsDto?> GetFundamentalsAsync(string symbol, CancellationToken ct = default);
    Task<TechnicalIndicatorsDto?> GetTechnicalIndicatorsAsync(string symbol, int days = 30, CancellationToken ct = default);
    Task<IEnumerable<StockSearchResultDto>> SearchStocksAsync(string query, int limit = 10, CancellationToken ct = default);
}
```

```csharp
// Bourse.Application/Interfaces/IWatchlistService.cs
public interface IWatchlistService
{
    Task<IEnumerable<WatchlistSummaryDto>> GetWatchlistsAsync(string userId, CancellationToken ct = default);
    Task<WatchlistDetailDto?> GetWatchlistAsync(Guid watchlistId, string userId, CancellationToken ct = default);
    Task<WatchlistSummaryDto> CreateWatchlistAsync(CreateWatchlistDto watchlist, string userId, CancellationToken ct = default);
    Task<WatchlistSummaryDto?> UpdateWatchlistAsync(Guid watchlistId, UpdateWatchlistDto watchlist, string userId, CancellationToken ct = default);
    Task<bool> DeleteWatchlistAsync(Guid watchlistId, string userId, CancellationToken ct = default);
    Task<WatchlistItemDto?> AddItemAsync(Guid watchlistId, AddWatchlistItemDto item, string userId, CancellationToken ct = default);
    Task<bool> RemoveItemAsync(Guid watchlistId, string symbol, string userId, CancellationToken ct = default);
}
```

### 5.4 Application DTOs

```csharp
// Bourse.Application/DTOs/ScreenerFilterDto.cs
public record ScreenerFilterDto
{
    // Price filters
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    
    // Market Cap filters
    public string? MarketCapCategory { get; init; }
    public decimal? MinMarketCap { get; init; }
    public decimal? MaxMarketCap { get; init; }
    
    // Classification filters
    public string? Sector { get; init; }
    public string? Industry { get; init; }
    public string? Exchange { get; init; }
    
    // Valuation filters
    public decimal? MinPE { get; init; }
    public decimal? MaxPE { get; init; }
    public decimal? MinPB { get; init; }
    public decimal? MaxPB { get; init; }
    public decimal? MinPS { get; init; }
    public decimal? MaxPS { get; init; }
    
    // Fundamental filters
    public decimal? MinEPS { get; init; }
    public decimal? MinROE { get; init; }
    public decimal? MinProfitMargin { get; init; }
    public decimal? MinRevenueGrowth { get; init; }
    
    // Dividend filters
    public decimal? MinDividendYield { get; init; }
    
    // Technical filters
    public decimal? MinRSI { get; init; }
    public decimal? MaxRSI { get; init; }
    public bool? AboveSMA50 { get; init; }
    public bool? BelowSMA200 { get; init; }
    public bool? MacdCrossover { get; init; }
    
    // Volume filters
    public long? MinVolume { get; init; }
    public decimal? MinVolumePercentChange { get; init; }
    
    // Performance filters
    public decimal? MinYTDReturn { get; init; }
    public decimal? Min1YearReturn { get; init; }
    public decimal? Near52WeekHighPercent { get; init; }
    public decimal? Near52WeekLowPercent { get; init; }
    
    // Financial Health filters
    public decimal? MaxDebtToEquity { get; init; }
    public decimal? MinCurrentRatio { get; init; }
}
```

```csharp
// Bourse.Application/DTOs/ScreenerResultDto.cs
public record ScreenerResultDto
{
    public IReadOnlyList<StockDto> Items { get; init; } = [];
    public PageInfoDto PageInfo { get; init; } = null!;
    public ScreenerFilterDto AppliedFilters { get; init; } = null!;
}

public record PageInfoDto
{
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public bool HasNextPage { get; init; }
    public bool HasPreviousPage { get; init; }
}
```

```csharp
// Bourse.Application/DTOs/StockDto.cs
public record StockDto
{
    public required string Symbol { get; init; }
    public required string CompanyName { get; init; }
    public required string Exchange { get; init; }
    public string? Sector { get; init; }
    public string? Industry { get; init; }
    public decimal? MarketCap { get; init; }
    public string? MarketCapCategory { get; init; }
    public decimal? CurrentPrice { get; init; }
    public decimal? PriceChange { get; init; }
    public decimal? PriceChangePercent { get; init; }
    public decimal? DayHigh { get; init; }
    public decimal? DayLow { get; init; }
    public decimal? Week52High { get; init; }
    public decimal? Week52Low { get; init; }
    public long? Volume { get; init; }
    public long? AvgVolume { get; init; }
    public decimal? Beta { get; init; }
    public DateTime? LastUpdated { get; init; }
    
    // Fundamental data (optional, included when viewing detail)
    public FundamentalsDto? Fundamentals { get; init; }
    
    // Latest technical indicators (optional)
    public TechnicalIndicatorLatestDto? TechnicalIndicators { get; init; }
}
```

---

## 6. Key Workflows & Sequence Diagrams

### 6.1 Stock Screening Flow

```
┌─────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│ Client  │      │ Blazor UI   │      │ API Layer   │      │ Application │      │   Domain    │
│         │      │ (Presentation)     │ (Infrastructure)    │   Service   │      │   Engine    │
└────┬────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘
     │                  │                    │                    │                    │
     │ 1. User applies  │                    │                    │                    │
     │    filters       │                    │                    │                    │
     │──────────────────>                    │                    │                    │
     │                  │                    │                    │                    │
     │                  │ 2. GET /api/screener/filter?minRSI=30   │                    │
     │                  │───────────────────>                    │                    │
     │                  │                    │                    │                    │
     │                  │                    │ 3. FilterStocksAsync(filter)            │
     │                  │                    │───────────────────>                    │
     │                  │                    │                    │                    │
     │                  │                    │                    │ 4. Filter(stocks, filter)
     │                  │                    │                    │───────────────────>
     │                  │                    │                    │                    │
     │                  │                    │                    │         5. Apply filters:
     │                  │                    │                    │         - Price range
     │                  │                    │                    │         - Market cap
     │                  │                    │                    │         - RSI
     │                  │                    │                    │         etc.
     │                  │                    │                    │                    │
     │                  │                    │                    │<───────────────────
     │                  │                    │                    │ 6. Return filtered stocks
     │                  │                    │<───────────────────                    │
     │                  │                    │                    │                    │
     │                  │ 7. Paginate & sort │                    │                    │
     │                  │    results         │                    │                    │
     │                  │                    │                    │                    │
     │                  │ 8. Return PageInfo + StockDtos          │                    │
     │                  │<───────────────────                    │                    │
     │                  │                    │                    │                    │
     │ 9. Display grid  │                    │                    │                    │
     │<──────────────────                    │                    │                    │
     │                  │                    │                    │                    │
```

### 6.2 Real-time Price Update Flow

```
┌─────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│ Client  │      │ SignalR Hub │      │ Market Data │      │ Data Sync   │      │ External    │
│ (Blazor)│      │             │      │   Service   │      │  Service    │      │  API        │
└────┬────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘
     │                  │                    │                    │                    │
     │ 1. Connect to hub│                    │                    │                    │
     │──────────────────>                    │                    │                    │
     │                  │                    │                    │                    │
     │ 2. Subscribe(AAPL,MSFT)               │                    │                    │
     │──────────────────>                    │                    │                    │
     │                  │                    │                    │                    │
     │                  │ 3. Register subscription              │                    │
     │                  │───────────────────>                    │                    │
     │                  │                    │                    │                    │
     │                  │                    │ 4. Background fetch│                    │
     │                  │                    │───────────────────>                    │
     │                  │                    │                    │                    │
     │                  │                    │ 5. GET /quote     │                    │
     │                  │                    │───────────────────────────────────────>
     │                  │                    │                    │                    │
     │                  │                    │ 6. Return quote   │                    │
     │                  │                    │<───────────────────────────────────────
     │                  │                    │                    │                    │
     │                  │                    │ 7. Update price   │                    │
     │                  │                    │<───────────────────                    │
     │                  │                    │                    │                    │
     │ 8. OnPriceUpdate │                    │                    │                    │
     │<──────────────────                    │                    │                    │
     │                  │                    │                    │                    │
     │ 9. Update UI     │                    │                    │                    │
     │<──────────────────                    │                    │                    │
     │                  │                    │                    │                    │
```

### 6.3 Data Synchronization Flow

```
┌─────────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│ Background  │      │ Data Sync   │      │ Repository  │      │ External    │      │   SQL       │
│  Service    │      │   Service   │      │ (EF Core)   │      │   API       │      │  Server     │
└──────┬──────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘
       │                   │                    │                    │                    │
       │ 1. Trigger (daily)│                    │                    │                    │
       │                   │                    │                    │                    │
       │ 2. Get all symbols│                    │                    │                    │
       │──────────────────>                    │                    │                    │
       │                   │                    │                    │                    │
       │                   │ 3. GetStocks()     │                    │                    │
       │                   │───────────────────>                    │                    │
       │                   │                    │                    │                    │
       │                   │ 4. SELECT * FROM Stocks                │                    │
       │                   │<───────────────────                    │                    │
       │                   │                    │                    │                    │
       │ 5. For each symbol:                    │                    │                    │
       │   Fetch quote    │                    │                    │                    │
       │──────────────────>                    │                    │                    │
       │                   │                    │ 6. GET /quote     │                    │
       │                   │                    │───────────────────────────────────────>
       │                   │                    │                    │                    │
       │                   │                    │ 7. Return data    │                    │
       │                   │                    │<───────────────────────────────────────
       │                   │                    │                    │                    │
       │                   │ 8. Update stock    │                    │                    │
       │                   │───────────────────>                    │                    │
       │                   │                    │                    │                    │
       │                   │                    │ 9. UPDATE Stocks  │                    │
       │                   │                    │───────────────────>                    │
       │                   │                    │                    │                    │
       │                   │                    │ 10. COMMIT        │                    │
       │                   │                    │<───────────────────                    │
       │                   │                    │                    │                    │
       │ 11. Calculate    │                    │                    │                    │
       │    indicators    │                    │                    │                    │
       │──────────────────>                    │                    │                    │
       │                   │                    │                    │                    │
       │                   │ 12. Store technical indicators        │                    │
       │                   │───────────────────>                    │                    │
       │                   │                    │                    │                    │
       │                   │                    │ 13. INSERT INTO TechnicalIndicators   │
       │                   │                    │───────────────────>                    │
       │                   │                    │                    │                    │
```

### 6.4 Watchlist CRUD Flow

```
┌─────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│ Client  │      │ API Layer   │      │ Application │      │ Repository  │      │   SQL       │
│         │      │             │      │   Service   │      │             │      │  Server     │
└────┬────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘      └──────┬──────┘
     │                  │                    │                    │                    │
     │ 1. POST /api/watchlists              │                    │                    │
     │   {name: Tech}   │                    │                    │                    │
     │──────────────────>                    │                    │                    │
     │                  │                    │                    │                    │
     │                  │ 2. CreateWatchlistAsync(dto, userId)   │                    │
     │                  │───────────────────>                    │                    │
     │                  │                    │                    │                    │
     │                  │                    │ 3. Validate user   │                    │
     │                  │                    │───────────────────>                    │
     │                  │                    │                    │                    │
     │                  │                    │ 4. Create entity   │                    │
     │                  │                    │                    │                    │
     │                  │                    │ 5. AddAsync(watchlist)                  │
     │                  │                    │───────────────────>                    │
     │                  │                    │                    │                    │
     │                  │                    │                    │ 6. INSERT         │
     │                  │                    │                    │───────────────────>│
     │                  │                    │                    │                    │
     │                  │                    │                    │ 7. Return new ID  │
     │                  │                    │<───────────────────                    │
     │                  │                    │                    │                    │
     │                  │ 8. Return DTO      │                    │                    │
     │                  │<───────────────────                    │                    │
     │                  │                    │                    │                    │
     │ 9. 201 Created   │                    │                    │                    │
     │<──────────────────                    │                    │                    │
     │                  │                    │                    │                    │
```

---

## 7. Error Handling Strategy

### 7.1 Exception Hierarchy

```csharp
// Bourse.Domain/Exceptions/DomainException.cs
public abstract class DomainException : Exception
{
    public string Code { get; }
    public IDictionary<string, object> Metadata { get; }
    
    protected DomainException(string code, string message, IDictionary<string, object>? metadata = null)
        : base(message)
    {
        Code = code;
        Metadata = metadata ?? new Dictionary<string, object>();
    }
}

// Specific exceptions
public class StockNotFoundException : DomainException
{
    public StockNotFoundException(string symbol)
        : base(nameof(StockNotFoundException), 
               message: $”Stock with symbol '{symbol}' was not found.”,
               metadata: new Dictionary<string, object> { { nameof(symbol), symbol } })
    { }
}

public class InvalidFilterException : DomainException
{
    public InvalidFilterException(string parameter, string reason)
        : base(nameof(InvalidFilterException),
               message: $”Invalid filter parameter '{parameter}': {reason}”,
               metadata: new Dictionary<string, object> { { nameof(parameter), parameter } })
    { }
}

public class WatchlistAccessDeniedException : DomainException
{
    public WatchlistAccessDeniedException(Guid watchlistId, string userId)
        : base(nameof(WatchlistAccessDeniedException),
               message: $”User '{userId}' does not have access to watchlist '{watchlistId}'.”,
               metadata: new Dictionary<string, object> { { nameof(watchlistId), watchlistId } })
    { }
}
```

### 7.2 Global Exception Handling

```csharp
// Bourse.Presentation/Middleware/ExceptionHandlingMiddleware.cs
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, “Domain exception: {Code}”, ex.Code);
            await HandleDomainExceptionAsync(context, ex);
        }
        catch (ExternalApiException ex)
        {
            _logger.LogError(ex, “External API error: {Code}”, ex.Code);
            await HandleExternalApiExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, “Unhandled exception”);
            await HandleUnhandledExceptionAsync(context, ex);
        }
    }
    
    private static async Task HandleDomainExceptionAsync(HttpContext context, DomainException ex)
    {
        context.Response.StatusCode = ex switch
        {
            StockNotFoundException => StatusCodes.Status404NotFound,
            InvalidFilterException => StatusCodes.Status400BadRequest,
            WatchlistAccessDeniedException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };
        
        context.Response.ContentType = “application/json”;
        
        var response = new ErrorResponse
        {
            Code = ex.Code,
            Message = ex.Message,
            Metadata = ex.Metadata
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### 7.3 API Error Response Format

```csharp
// Common error response structure
public record ErrorResponse
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public IDictionary<string, object>? Metadata { get; init; }
    public string? TraceId { get; init; }
    public string? DocumentationUrl { get; init; }
}

// Example error response
{
  "code": "StockNotFoundException",
  "message": "Stock with symbol 'INVALID' was not found.",
  "metadata": {
    "symbol": "INVALID"
  },
  "traceId": "00-abc123def456-01",
  "documentationUrl": "https://api.bourse.com/docs/errors/StockNotFoundException"
}
```

### 7.4 Retry Policy for External APIs

```csharp
// Bourse.Infrastructure/External/RetryPolicyConfiguration.cs
public static class RetryPolicyConfiguration
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode == (HttpStatusCode)429 || // Too Many Requests
                          r.StatusCode == (HttpStatusCode)502 || // Bad Gateway
                          r.StatusCode == (HttpStatusCode)503)   // Service Unavailable
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Log.Warning(“Retry {RetryAttempt} after {Timespan}s due to {Error}”,
                        retryAttempt, timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });
    }
    
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .OrResult(r => r.StatusCode >= HttpStatusCode.InternalServerError)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (outcome, breakDelay) =>
                {
                    Log.Warning(“Circuit breaker opened for {BreakDelay}s”, breakDelay.TotalSeconds);
                },
                onReset: () =>
                {
                    Log.Information(“Circuit breaker reset”);
                });
    }
}
```

### 7.5 API Error Codes Reference

| Error Code | HTTP Status | Description | Resolution |
|------------|-------------|-------------|------------|
| `StockNotFoundException` | 404 | Stock symbol not found in database | Verify symbol is valid and data is loaded |
| `InvalidFilterException` | 400 | Filter parameter validation failed | Check parameter bounds (e.g., RSI 0-100) |
| `WatchlistAccessDeniedException` | 403 | User doesn't own this watchlist | Ensure user owns the resource |
| `RateLimitExceeded` | 429 | Too many requests | Wait and retry with exponential backoff |
| `ExternalApiUnavailable` | 503 | External data provider is down | Use cached data; retry later |
| `InvalidPageParameters` | 400 | Page number or size out of range | Ensure page >= 1, pageSize 1-200 |
| `DuplicateWatchlistItem` | 409 | Stock already in watchlist | Stock already exists in this watchlist |
| `UnauthorizedAccess` | 401 | Missing or invalid JWT token | Re-authenticate and obtain valid token |

---

## 8. Security Design

### 8.1 Authentication & Authorization

```csharp
// Programme.cs in API project
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration[“Jwt:Issuer”],
            ValidAudience = builder.Configuration[“Jwt:Audience”],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration[“Jwt:Key”] ?? throw new InvalidOperationException()))
        };
        
        // Allow SignalR to use JWT from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query[“access_token”];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(“/hubs”))
                {
                    context.Token = accessToken;
                }
                
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.ViewerPolicy, policy => 
        policy.RequireRole(RoleNames.Viewer, RoleNames.Editor, RoleNames.Admin));
    
    options.AddPolicy(PolicyNames.EditorPolicy, policy => 
        policy.RequireRole(RoleNames.Editor, RoleNames.Admin));
    
    options.AddPolicy(PolicyNames.AdminPolicy, policy => 
        policy.RequireRole(RoleNames.Admin));
});
```

### 8.2 Authorization Policies for API Endpoints

```csharp
// Watchlist authorization
[Authorize(Policy = PolicyNames.ViewerPolicy)]
public class WatchlistsController : ControllerBase
{
    // All users can view watchlists
    
    [Authorize(Policy = PolicyNames.EditorPolicy)]
    public async Task<ActionResult<WatchlistDto>> Create([FromBody] CreateWatchlistDto dto)
    {
        // Only editors and admins can create
    }
}

// Screener - public for viewing, but presets are user-specific
[Authorize(Policy = PolicyNames.ViewerPolicy)]
public class ScreenerController : ControllerBase
{
    // Filter is public (but user-specific results)
}
```

### 8.3 Input Validation

```csharp
// Request validation using FluentValidation
public class ScreenerFilterValidator : AbstractValidator<ScreenerFilterDto>
{
    public ScreenerFilterValidator()
    {
        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);
        
        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice ?? 0)
            .When(x => x.MaxPrice.HasValue);
        
        RuleFor(x => x.MinRSI)
            .InclusiveBetween(0, 100)
            .When(x => x.MinRSI.HasValue);
        
        RuleFor(x => x.MaxRSI)
            .InclusiveBetween(0, 100)
            .When(x => x.MaxRSI.HasValue);
        
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200);
        
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1);
    }
}
```

### 8.4 Rate Limiting

```csharp
// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.AddFixedWindowLimiter(“api”, limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
    
    options.AddFixedWindowLimiter(“screener”, limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
    });
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.Headers[“Retry-After”] = “60”;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            code = “RateLimitExceeded”,
            message = “Too many requests. Please try again later.”
        }, token);
    };
});
```

### 8.5 Data Protection

- **HTTPS:** All communication must be over HTTPS in production
- **Sensitive Data:** API keys stored in Azure Key Vault / AWS Secrets Manager
- **Database:** SQL Server TDE (Transparent Data Encryption) enabled
- **Logging:** No sensitive data (passwords, API keys) in logs

### 8.6 CORS Configuration

```csharp
// Configure CORS for Blazor WASM client
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins(
                "https://localhost:5001",
                "https://localhost:5002",
                "https://bourseapp.azurewebsites.net")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // Required for SignalR
            .WithExposedHeaders("Retry-After");
    });
});

// Apply CORS middleware
app.UseCors("BlazorClient");
```

### 8.8 API Versioning Strategy

All API endpoints use URL-based versioning with a `v1` prefix:

```
https://api.bourse.com/api/v1/screener/filter
https://api.bourse.com/api/v1/stocks/{symbol}
```

**Versioning Rules:**
- Breaking changes require a new version (v2, v3)
- Non-breaking additions are added to current version
- Old versions supported for minimum 12 months
- Version deprecation announced via headers and response metadata

**Version Header Response:**
```http
X-API-Version: 1.0
X-API-Deprecation-Date: 2026-01-01
```

### 8.9 Development Authentication

For development/testing, a special test mode is available:

```csharp
// Development authentication bypass (only in Development environment)
if (builder.Environment.IsDevelopment())
{
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
            // Dev mode: Accept any valid JWT structure
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = false,
                SignatureValidator = (token, parameters) => 
                    new JwtSecurityToken(token)
            };
        });
}

// Dev test user seeding
public static async Task SeedTestUsers(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!context.Users.Any())
    {
        context.Users.AddRange(
            new ApplicationUser { Id = "dev-user-1", Email = "dev@test.com", UserName = "dev-user" },
            new ApplicationUser { Id = "dev-user-2", Email = "test@test.com", UserName = "test-user" }
        );
        await context.SaveChangesAsync();
    }
}
```

---

### 8.10 SignalR Reconnection Strategy

```javascript
// Comprehensive SignalR connection management
class MarketDataConnectionManager {
    constructor() {
        this.subscribedSymbols = new Set();
        this.connection = null;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
    }
    
    async connect() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/market-data', {
                accessTokenFactory: () => getJwtToken()
            })
            .withAutomaticReconnect([0, 1000, 5000, 10000, 30000]) // Exponential backoff
            .configureLogging(signalR.LogLevel.Information)
            .build();
        
        // Handle connection state changes
        this.connection.onclose((error) => {
            console.log('Connection closed:', error);
            this.onConnectionClosed();
        });
        
        this.connection.onreconnecting((error) => {
            console.log('Reconnecting:', error);
            this.onConnectionStateChanged('Reconnecting');
        });
        
        this.connection.onreconnected((connectionId) => {
            console.log('Reconnected:', connectionId);
            this.reconnectAttempts = 0;
            this.onConnectionStateChanged('Connected');
            this.resubscribeSymbols(); // Re-subscribe after reconnect
        });
        
        await this.connection.start();
    }
    
    async subscribe(symbols) {
        symbols.forEach(s => this.subscribedSymbols.add(s));
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('SubscribeToSymbols', symbols);
        }
    }
    
    async resubscribeSymbols() {
        // Re-subscribe all previously subscribed symbols on reconnect
        if (this.subscribedSymbols.size > 0) {
            await this.connection.invoke('SubscribeToSymbols', 
                Array.from(this.subscribedSymbols));
        }
    }
    
    onConnectionStateChanged(state) {
        // Update UI with connection status
        signalRService.connectionState.set(state);
    }
}
```

---

## 9. Configuration Management

### 9.1 Configuration Schema (appsettings.json)

```json
{
  \"Logging\": {
    \"LogLevel\": {
      \"Default\": \"Information\",
      \"Microsoft.AspNetCore\": \"Warning\",
      \"Microsoft.EntityFrameworkCore\": \"Warning\"
    }
  },
  
  \"AllowedHosts\": \"*\",
  
  \"ConnectionStrings\": {
    \"DefaultConnection\": \"Server=localhost;Database=BourseDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true\",
    \"ReadReplicaConnection\": \"Server=read-replica;Database=BourseDb;Trusted_Connection=True;TrustServerCertificate=True\"
  },
  
  \"Jwt\": {
    \"Key\": \"[REPLACE_WITH_32+_CHARACTER_SECRET_KEY]\",
    \"Issuer\": \"BourseStockScreener\",
    \"Audience\": \"BourseStockScreenerClients\",
    \"ExpirationMinutes\": 60
  },
  
  \"ExternalApis\": {
    \"Finnhub\": {
      \"BaseUrl\": \"https://finnhub.io/api/v1\",
      \"ApiKey\": \"[REPLACE_WITH_FINNHUB_API_KEY]\",
      \"RateLimitCallsPerMinute\": 60,
      \"TimeoutSeconds\": 30
    },
    \"AlphaVantage\": {
      \"BaseUrl\": \"https://www.alphavantage.co/query\",
      \"ApiKey\": \"[REPLACE_WITH_ALPHA_VANTAGE_API_KEY]\",
      \"RateLimitCallsPerDay\": 25,
      \"TimeoutSeconds\": 30
    }
  },
  
  \"DataSync\": {
    \"Enabled\": true,
    \"DailySyncTime\": \"02:00:00\",
    \"SyncIntervalMinutes\": 15,
    \"BatchSize\": 100
  },
  
  \"SignalR\": {
    \"MaxConnections\": 5000,
    \"KeepAliveIntervalSeconds\": 15,
    \"ClientTimeoutSeconds\": 30
  },
  
  \"Caching\": {
    \"DefaultExpirationMinutes\": 5,
    \"PriceDataExpirationSeconds\": 60,
    \"StockDetailExpirationMinutes\": 15
  },
  
  \"RateLimiting\": {
    \"PermitLimit\": 100,
    \"WindowMinutes\": 1
  }
}
```

### 9.2 Environment-Specific Configuration

```bash
# Development
ASPNETCORE_ENVIRONMENT=Development

# Staging
ASPNETCORE_ENVIRONMENT=Staging

# Production
ASPNETCORE_ENVIRONMENT=Production
```

```json
// appsettings.Development.json
{
  \"Logging\": {
    \"LogLevel\": {
      \"Default\": \"Debug\"
    }
  },
  \"ExternalApis\": {
    \"Finnhub\": {
      \"ApiKey\": \"test-key\"
    }
  }
}
```

### 9.3 Secrets Management (for production)

```yaml
# azure-pipelines.yml example for Azure
variables:
  - name: finnhubApiKey
    type: secret
    mask: true
    
steps:
  - task: AzureKeyVault@2
    inputs:
      azureSubscription: 'AzureServiceConnection'
      KeyVaultName: 'BourseKeyVault'
      SecretsFilter: 'FinnhubApiKey,SqlConnectionString'
      RunAsPolicy: false
      
  - task: .NET Core CLI@2
    env:
      FinnhubApiKey: $(FinnhubApiKey)
```

---

## 10. Deployment Architecture

### 10.1 Azure Infrastructure (Recommended)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              AZURE REGION                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                     AZURE APP SERVICE (API)                         │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                  │   │
│  │  │  Slot: Dev  │  │  Slot: Stage│  │  Production │                  │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘                  │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                     │                                       │
│  ┌──────────────────────────────────┼──────────────────────────────────┐   │
│  │                         AZURE FRONT DOOR                             │   │
│  │                    (WAF + CDN + Load Balancer)                       │   │
│  └──────────────────────────────────┼──────────────────────────────────┘   │
│                                     │                                       │
│  ┌──────────────────────────────────┼──────────────────────────────────┐   │
│  │                    BLAZOR WEBASSEMBLY (Static)                      │   │
│  │                    Blob Storage + Azure CDN                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                     │                                       │
├─────────────────────────────────────┼─────────────────────────────────────┤
│                                     │                                       │
│  ┌──────────────┐  ┌──────────────┐  │  ┌──────────────┐  ┌────────────┐  │
│  │   Azure SQL  │  │    Azure     │  │  │ Azure Redis  │  │   Azure    │  │
│  │    Server    │  │   Key Vault  │  │  │   (Cache)    │  │  Monitor   │  │
│  │  (Database)  │  │  (Secrets)   │  │  └──────────────┘  └────────────┘  │
│  └──────────────┘  └──────────────┘  │                                       │
└──────────────────────────────────────┼───────────────────────────────────────┘
                                       │
┌──────────────────────────────────────┼───────────────────────────────────────┐
│                           EXTERNAL SERVICES                                 │
│  ┌──────────────┐  ┌──────────────┐  │  ┌──────────────┐                    │
│  │   Finnhub    │  │ Alpha Vantage│  │  │    Email     │                    │
│  └──────────────┘  └──────────────┘  │  └──────────────┘                    │
└──────────────────────────────────────┴───────────────────────────────────────┘
```

### 10.2 Docker Compose (Development)

```yaml
# docker-compose.yml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: src/Bourse.Presentation/Dockerfile
    ports:
      - \"5000:8080\"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=BourseDb;User=sa;Password=YourPassword123
      - FinnhubApiKey=${FINNHUB_API_KEY}
    depends_on:
      - sqlserver
      - redis
    networks:
      - bourse-network

  blazor:
    build:
      context: .
      dockerfile: src/Bourse.Presentation/Dockerfile
    ports:
      - \"5001:8080\"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - api
    networks:
      - bourse-network

  sqlserver:
    image: mcr.microsoft.com/azure-sql-edge:latest
    ports:
      - \"1433:1433\"
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourPassword123
      - MSSQL_PID=Developer
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - bourse-network

  redis:
    image: redis:alpine
    ports:
      - \"6379:6379\"
    networks:
      - bourse-network

volumes:
  sqlserver-data:

networks:
  bourse-network:
    driver: bridge
```

### 10.3 Kubernetes Manifest (Production)

```yaml
# k8s/deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: bourse-api
  namespace: bourse
spec:
  replicas: 3
  selector:
    matchLabels:
      app: bourse-api
  template:
    metadata:
      labels:
        app: bourse-api
    spec:
      containers:
        - name: api
          image: bourse.azurecr.io/api:${VERSION}
          ports:
            - containerPort: 8080
          env:
            - name: ConnectionStrings__DefaultConnection
              valueFrom:
                secretKeyRef:
                  name: bourse-secrets
                  key: connection-string
            - name: FinnhubApiKey
              valueFrom:
                secretKeyRef:
                  name: bourse-secrets
                  key: finnhub-api-key
          resources:
            requests:
              memory: \"256Mi\"
              cpu: \"100m\"
            limits:
              memory: \"1Gi\"
              cpu: \"500m\"
          readinessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 10
            periodSeconds: 5
          livenessProbe:
            httpGet:
              path: /health
              port: 8080
            initialDelaySeconds: 30
            periodSeconds: 15
---
apiVersion: v1
kind: Service
metadata:
  name: bourse-api-service
  namespace: bourse
spec:
  selector:
    app: bourse-api
  ports:
    - port: 80
      targetPort: 8080
  type: LoadBalancer
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: bourse-api-hpa
  namespace: bourse
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: bourse-api
  minReplicas: 3
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
```

### 10.4 Health Check Endpoints

```csharp
// Health checks for the API
builder.Services.AddHealthChecks()
    .AddCheck(“self”, () => HealthCheckResult.Healthy())
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString(“DefaultConnection”)!,
        name: “database”,
        tags: new[] { “db”, “sql”, “ready” })
    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString(“Redis”),
        name: “cache”,
        tags: new[] { “cache”, “ready” })
    .AddUrlGroup(
        uri: new Uri(“https://finnhub.io/api/v1/health”),
        name: “finnhub-api”,
        tags: new[] { “external”, “ready” });
```

---

## Appendix A: Domain Entity Relationships

```
┌─────────────┐          ┌──────────────────┐          ┌────────────────────────┐
│   Stocks    │ 1 ──────< │  Fundamentals    │          │  TechnicalIndicators   │
│             │          │                  │          │                        │
│ Id (PK)     │          │ Id (PK)          │          │ Id (PK)                │
│ Symbol      │          │ StockId (FK)     │          │ StockId (FK)           │
│ CompanyName │          │ PE_Ratio         │          │ TradeDate              │
│ Exchange    │          │ PB_Ratio         │          │ RSI_14                 │
│ Sector      │          │ ...              │          │ SMA_20                 │
│ Industry    │          └──────────────────┘          │ SMA_50                 │
│ MarketCap   │                  │                    │ SMA_200                │
│ CurrentPrice│                  │ 1:1                │ MACD                   │
│ ...         │                  │                   │ ...                    │
└──────┬──────┘                  │                   └────────────┬─────────────┘
       │                         │                              │
       │ 1:N                     │                              │
       │                         │                              │
       ▼                         │                              ▼
┌─────────────┐                  │                      ┌────────────────────────┐
│PriceHistory │                  │                      │     WatchlistItems     │
│             │                  │                      │                        │
│ Id (PK)     │                  │                      │ Id (PK)                │
│ StockId(FK) │                  │                      │ WatchlistId (FK)       │
│ TradeDate   │                  │                      │ StockId (FK)           │
│ Open        │                  │                      │ SharesOwned            │
│ High        │                  │                      │ CostBasis              │
│ Low         │                  │                      └───────────┬────────────┘
│ Close       │                  │                              │
│ Volume      │                  │                              │ N:1
└─────────────┘                  │                              ▼
                         ┌───────┴─────────────┐       ┌────────────────────────┐
                         │    Watchlists      │       │        Stocks          │
                         │                    │       │                        │
                         │ Id (PK)            │       └────────────────────────┘
                         │ Name               │
                         │ UserId             │
                         └────────────────────┘
```

---

*Document Version: 1.0 | Status: Draft | Last Updated: January 2025*