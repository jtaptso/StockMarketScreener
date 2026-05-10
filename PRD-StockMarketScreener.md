# Stock Market Screener - Product Requirement Document (PRD)

**Version:** 1.0  
**Author:** Product Team  
**Date:** January 2025  
**Tech Stack:** .NET 10 (API) | Blazor (UI) | SQL Server (Database)

---

## 1. Overview

### 1.1 Purpose
A stock market screener application that enables investors to filter and analyze publicly traded securities based on fundamental and technical criteria, helping them identify potential investment opportunities.

### 1.2 Target Users
- **Retail Investors:** Self-directed investors seeking to filter stocks based on personal criteria
- **Active Traders:** Users who need real-time or near-real-time data for intraday decisions
- **Financial Analysts:** Professionals using screeners for preliminary stock research

### 1.3 Success Metrics
- Filter results returned within 3 seconds for standard queries
- Support for minimum 5,000+ stocks in the screening database
- 99.5% uptime for the screening service

---

## 2. Technical Architecture

### 2.1 Clean Architecture Overview

This project follows **Clean Architecture** principles (Robert C. Martin / Uncle Bob) with clear separation of concerns across four main layers. Each layer has specific responsibilities and can be tested independently.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PRESENTATION LAYER                                │
│                   (Blazor WebAssembly - .NET 10)                            │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │  Pages          Components           ViewModels        Services      │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                       │
│                                    ▼                                       │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │                  Application Layer (Use Cases)                       │   │
│   │   DTOs              Interfaces (Ports)          Services (Use Cases) │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                       │
│                                    ▼                                       │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │                       DOMAIN LAYER (Core)                            │   │
│   │  Entities         Enums            Value Objects      Domain Services │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
│                                    ▲                                       │
│                                    │                                       │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │                    INFRASTRUCTURE LAYER                              │   │
│   │  EF Core DbContext    Repositories     API Clients    SignalR Hub    │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Project Structure

```
src/
├── Bourse.Domain/                          # Core business logic - NO external dependencies
│   │
│   ├── Entities/
│   │   ├── Stock.cs
│   │   ├── PriceHistory.cs
│   │   ├── Fundamentals.cs
│   │   ├── TechnicalIndicators.cs
│   │   ├── Watchlist.cs
│   │   ├── WatchlistItem.cs
│   │   └── FilterPreset.cs
│   │
│   ├── Enums/
│   │   ├── Exchange.cs
│   │   ├── MarketCapCategory.cs
│   │   ├── SortOrder.cs
│   │   └── IndicatorType.cs
│   │
│   ├── ValueObjects/
│   │   ├── Money.cs                      # Immutable decimal with currency
│   │   ├── DateRange.cs                  # Date range for queries
│   │   └── Percentage.cs                 # Percentage with validation
│   │
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IStockRepository.cs
│   │   │   ├── IPriceHistoryRepository.cs
│   │   │   ├── IFundamentalsRepository.cs
│   │   │   └── IWatchlistRepository.cs
│   │   │
│   │   └── Services/
│   │       ├── IIndicatorCalculator.cs
│   │       ├── IScreenerEngine.cs
│   │       └── IMarketDataProvider.cs    # Port for external data
│   │
│   └── Services/
│       ├── IndicatorCalculator.cs        # RSI, MACD, SMA, etc.
│       └── ScreenerEngine.cs             # Core filtering logic
│
├── Bourse.Application/                    # Application logic - depends only on Domain
│   │
│   ├── DTOs/
│   │   ├── StockDto.cs
│   │   ├── PriceHistoryDto.cs
│   │   ├── FundamentalsDto.cs
│   │   ├── ScreenerFilterDto.cs
│   │   ├── ScreenerResultDto.cs
│   │   └── WatchlistDto.cs
│   │
│   ├── Interfaces/
│   │   ├── IScreenerService.cs
│   │   ├── IStockService.cs
│   │   ├── IMarketDataService.cs
│   │   └── IWatchlistService.cs
│   │
│   └── Services/
│       ├── ScreenerService.cs
│       ├── StockService.cs
│       ├── MarketDataService.cs
│       └── WatchlistService.cs
│
├── Bourse.Infrastructure/                 # External concerns - depends on Domain & Application
│   │
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── StockEntityConfiguration.cs
│   │   │   ├── PriceHistoryConfiguration.cs
│   │   │   ├── FundamentalsConfiguration.cs
│   │   │   └── TechnicalIndicatorsConfiguration.cs
│   │   │
│   │   └── Repositories/
│   │       ├── StockRepository.cs
│   │       ├── PriceHistoryRepository.cs
│   │       ├── FundamentalsRepository.cs
│   │       └── WatchlistRepository.cs
│   │
│   ├── External/
│   │   ├── FinnhubClient.cs              # Implements IMarketDataProvider
│   │   ├── AlphaVantageClient.cs         # Alternative data source
│   │   └── Models/
│   │       ├── FinnhubQuoteResponse.cs
│   │       └── FinnhubCandleResponse.cs
│   │
│   ├── Services/
│   │   ├── CachingService.cs
│   │   ├── SignalRMarketDataService.cs
│   │   └── BackgroundDataSyncService.cs  # Hosted service for data updates
│   │
│   └── Mapping/
│       └── MappingProfile.cs             # AutoMapper profiles
│
├── Bourse.Presentation/                   # Blazor WebAssembly - depends on Application
│   │
│   ├── App.razor
│   ├── Program.cs
│   │
│   ├── Pages/
│   │   ├── Screener.razor
│   │   ├── StockDetail.razor
│   │   ├── Watchlist.razor
│   │   └── Settings.razor
│   │
│   ├── Components/
│   │   ├── Filters/
│   │   │   ├── FilterPanel.razor
│   │   │   ├── PriceFilter.razor
│   │   │   ├── MarketCapFilter.razor
│   │   │   ├── TechnicalFilter.razor
│   │   │   └── ValuationFilter.razor
│   │   │
│   │   ├── Charts/
│   │   │   ├── CandlestickChart.razor
│   │   │   └── PriceLineChart.razor
│   │   │
│   │   ├── Grid/
│   │   │   ├── StockGrid.razor
│   │   │   └── StockGridRow.razor
│   │   │
│   │   └── Common/
│   │       ├── PriceDisplay.razor
│   │       ├── LoadingSpinner.razor
│   │       └── ConnectionStatus.razor
│   │
│   ├── ViewModels/
│   │   ├── ScreenerViewModel.cs
│   │   ├── StockDetailViewModel.cs
│   │   └── WatchlistViewModel.cs
│   │
│   ├── Services/
│   │   ├── ApiClient.cs
│   │   ├── SignalRService.cs
│   │   └── LocalStorageService.cs
│   │
│   ├── _Imports.razor
│   └── wwwroot/
│       ├── index.html
│       └── css/
│
└── Bourse.Tests/                          # Unit & integration tests
    ├── Domain/
    │   ├── IndicatorCalculatorTests.cs
    │   └── ScreenerEngineTests.cs
    │
    ├── Application/
    │   ├── ScreenerServiceTests.cs
    │   └── StockServiceTests.cs
    │
    └── Infrastructure/
        └── RepositoryTests.cs
```

### 2.3 Dependency Rules

| Layer | Can Depend On | Cannot Depend On |
|-------|---------------|------------------|
| **Domain** | None (pure) | Application, Infrastructure, Presentation |
| **Application** | Domain | Infrastructure, Presentation |
| **Infrastructure** | Domain, Application | Presentation |
| **Presentation** | Domain (via Application) | None (but typically uses Application DTOs) |

### 2.4 Namespace Conventions

```csharp
// Domain
Bourse.Domain.Entities
Bourse.Domain.Enums
Bourse.Domain.ValueObjects
Bourse.Domain.Interfaces.Repositories
Bourse.Domain.Interfaces.Services
Bourse.Domain.Services

// Application
Bourse.Application.DTOs
Bourse.Application.Interfaces
Bourse.Application.Services

// Infrastructure
Bourse.Infrastructure.Persistence
Bourse.Infrastructure.Persistence.Repositories
Bourse.Infrastructure.External
Bourse.Infrastructure.Services
Bourse.Infrastructure.Mapping

// Presentation (Blazor)
Bourse.Presentation.Pages
Bourse.Presentation.Components
Bourse.Presentation.ViewModels
Bourse.Presentation.Services
```

### 2.5 Key Architecture Decisions

| Decision | Rationale |
|----------|------------|
| **Domain has no dependencies** | Ensures business rules are testable and stable |
| **Application depends only on Domain** | Use cases remain framework-agnostic |
| **Infrastructure implements Application interfaces** | Allows swapping data sources without changing business logic |
| **Presentation uses DTOs, not Domain entities** | Protects core from UI changes; allows API evolution |
| **SignalR lives in Infrastructure** | SignalR is a delivery mechanism, not business logic |

### 2.6 .NET 10 Specific Features

| Feature | Where Applied |
|---------|---------------|
| **Primary Constructors** | Services in Application layer for cleaner DI |
| **Collection Expressions** | Creating lists in DTOs and domain methods |
| **New LINQ Methods** | `CountBy`, `AggregateBy` for grouping operations |
| **Required Members** | DTOs with mandatory fields |
| **Interceptor Overrides** | EF Core interceptors for audit fields |

---

### 2.7 System Architecture (Runtime View)

```
┌─────────────────────────────────────────────────────────────────┐
│                        Client (Blazor WASM)                     │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │ Screener UI │  │ Chart View  │  │ Watchlist / Portfolio   │  │
│  └──────┬──────┘  └──────┬──────┘  └───────────┬─────────────┘  │
└─────────┼────────────────┼────────────────────┼─────────────────┘
          │                │                    │
          └────────────────┼────────────────────┘
                           │ HTTP/REST + SignalR
┌──────────────────────────┼──────────────────────────────────────┐
│                     API Layer (.NET 10)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │ Screener    │  │ Market Data │  │ Real-time Updates       │  │
│  │ Controller  │  │ Controller  │  │ (SignalR Hub)           │  │
│  └──────┬──────┘  └──────┬──────┘  └───────────┬─────────────┘  │
│         │                │                     │                │
│         │       Application Layer (Use Cases)  │                │
│         │                │                     │                │
│         └────────────────┼─────────────────────┘                │
│                          │                                      │
│                   Domain Layer (Core Business Logic)            │
│         ┌────────────────┼─────────────────────┐               │
│         │                │                     │                │
│         ▼                ▼                     ▼                │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │ Screener    │  │ Indicator   │  │ Stock                  │  │
│  │ Engine      │  │ Calculator  │  │ Repository             │  │
│  └─────────────┘  └─────────────┘  └───────────┬─────────────┘  │
└─────────┼────────────────┼────────────────────┼─────────────────┘
          │                │                    │
          └────────────────┼────────────────────┘
                           │
┌──────────────────────────┼──────────────────────────────────────┐
│                   Infrastructure Layer                           │
│  ┌───────────────────────┼───────────────────┐                  │
│  │                       │                   │                  │
│  ▼                       ▼                   ▼                  │
│ ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│ │ SQL Server  │  │   Finnhub   │  │      SignalR            │  │
│ │   (EF Core) │  │   API       │  │   (Real-time prices)    │  │
│ └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 Technology Stack

| Layer | Technology | Version |
|-------|------------|---------|
| **Frontend** | Blazor WebAssembly | .NET 10 |
| **UI Components** | Syncfusion Blazor UI Suite | Latest |
| **API** | ASP.NET Core Web API | .NET 10 |
| **Real-time** | SignalR | .NET 10 |
| **Database** | SQL Server | 2022 |
| **ORM** | Entity Framework Core | 8.x |
| **Caching** | Redis (optional) | Latest |
| **DI Container** | Microsoft.Extensions.DI | Built-in |

---

## 3. Data Model

### 3.1 Database Schema

#### Stocks (Master Table)
| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key (Identity) |
| Symbol | NVARCHAR(10) | Stock ticker (e.g., AAPL) - Unique |
| CompanyName | NVARCHAR(200) | Full company name |
| Exchange | NVARCHAR(50) | Exchange (NYSE, NASDAQ, etc.) |
| Sector | NVARCHAR(100) | Industry sector |
| Industry | NVARCHAR(100) | Sub-industry |
| MarketCap | DECIMAL(18,2) | Market capitalization in USD |
| CurrentPrice | DECIMAL(18,4) | Latest price |
| LastUpdated | DATETIME2 | Timestamp of last update |

#### PriceHistory
| Column | Type | Description |
|--------|------|-------------|
| Id | BIGINT | Primary Key (Identity) |
| StockId | INT | FK to Stocks |
| TradeDate | DATE | Date of price data |
| OpenPrice | DECIMAL(18,4) | Opening price |
| HighPrice | DECIMAL(18,4) | High price |
| LowPrice | DECIMAL(18,4) | Low price |
| ClosePrice | DECIMAL(18,4) | Closing price |
| Volume | BIGINT | Trading volume |

#### Fundamentals
| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary Key |
| StockId | INT | FK to Stocks (Unique) |
| PE_Ratio | DECIMAL(18,4) | Price-to-Earnings |
| PB_Ratio | DECIMAL(18,4) | Price-to-Book |
| PS_Ratio | DECIMAL(18,4) | Price-to-Sales |
| EPS | DECIMAL(18,4) | Earnings per share |
| DividendYield | DECIMAL(10,4) | Dividend yield % |
| Beta | DECIMAL(10,4) | Beta coefficient |
| DebtToEquity | DECIMAL(18,4) | Debt-to-equity ratio |
| ROE | DECIMAL(10,4) | Return on Equity % |
| RevenueGrowth | DECIMAL(10,4) | Revenue growth % |
| ProfitMargin | DECIMAL(10,4) | Net profit margin % |

#### TechnicalIndicators (Calculated Daily)
| Column | Type | Description |
|--------|------|-------------|
| Id | BIGINT | Primary Key |
| StockId | INT | FK to Stocks |
| TradeDate | DATE | Reference date |
| RSI_14 | DECIMAL(10,4) | 14-day RSI |
| SMA_20 | DECIMAL(18,4) | 20-day Simple MA |
| SMA_50 | DECIMAL(18,4) | 50-day Simple MA |
| SMA_200 | DECIMAL(18,4) | 200-day Simple MA |
| MACD | DECIMAL(18,4) | MACD value |
| MACD_Signal | DECIMAL(18,4) | MACD signal line |
| MACD_Histogram | DECIMAL(18,4) | MACD histogram |
| ATR_14 | DECIMAL(18,4) | Average True Range |
| BB_Upper | DECIMAL(18,4) | Bollinger Upper |
| BB_Lower | DECIMAL(18,4) | Bollinger Lower |
| Volume | BIGINT | Trading volume |
| AvgVolume_20 | DECIMAL(18,4) | 20-day avg volume |

### 3.2 Indexes
- `IX_Stocks_Symbol` on Stocks(Symbol)
- `IX_PriceHistory_StockId_Date` on PriceHistory(StockId, TradeDate)
- `IX_TechnicalIndicators_StockId_Date` on TechnicalIndicators(StockId, TradeDate)
- `IX_Fundamentals_StockId` on Fundamentals(StockId)

---

## 4. Functionality Specification

### 4.1 Core Features

#### F1: Stock Screening Engine
**Priority:** P0 (Critical)

**Filter Categories:**

| Category | Filters |
|----------|---------|
| **Price Range** | Min/Max price |
| **Market Cap** | Micro (<$300M), Small ($300M-$2B), Mid ($2B-$10B), Large ($10B-$200B), Mega (>$200B) |
| **Valuation** | P/E ratio (min/max), P/B ratio, P/S ratio |
| **Profitability** | ROE (min %), Profit Margin (min %), EPS (min $) |
| **Growth** | Revenue Growth (min %), EPS Growth |
| **Dividends** | Dividend Yield (min %), Ex-Dividend date |
| **Financial Health** | Debt-to-Equity (max), Current Ratio |
| **Technical** | RSI (overbought/oversold), Price vs SMA (above/below), MACD crossover |
| **Volume** | Average Volume (min), Volume % change |
| **Performance** | YTD return, 1-Year return, 52-Week High/Low proximity |

**Screener Operations:**
- Combine multiple filters with AND logic
- Save filter presets for quick access
- Sort results by any column (ascending/descending)
- Export results to CSV

#### F2: Real-time Market Data
**Priority:** P0 (Critical)

- WebSocket connection via SignalR for live price updates
- Configurable refresh intervals (1s, 5s, 15s, 30s, 1m)
- Visual indication of price changes (green up, red down)
- Connection status indicator

#### F3: Interactive Charts
**Priority:** P1 (High)

- Candlestick chart with OHLC data
- Line chart for price history
- Volume bars overlay
- Technical indicators overlay (MA, Bollinger Bands)
- Time range selection: 1D, 1W, 1M, 3M, 6M, 1Y, 5Y, MAX
- Zoom and pan functionality
- Crosshair with price/date tooltip

#### F4: Stock Detail View
**Priority:** P1 (High)

- Company overview and business description
- Key statistics dashboard
- Fundamental data display
- Technical chart (embedded)
- News feed (future enhancement)

#### F5: Watchlist Management
**Priority:** P2 (Medium)

- Create multiple named watchlists
- Add/remove stocks to watchlists
- Quick filter by watchlist
- Position tracking (shares owned, cost basis)

#### F6: Data Management
**Priority:** P1 (High)

- Background job for daily data sync
- Incremental updates for price data
- Full refresh capability
- Data quality indicators

### 4.2 API Endpoints

#### Screener API
```
GET  /api/screener/filter
     ?minPrice=&maxPrice=
     &minMarketCap=&maxMarketCap=
     &minPE=&maxPE=
     &sector=&industry=
     &minRSI=&maxRSI=
     &sortBy=MarketCap&sortOrder=desc
     &page=1&pageSize=50

GET  /api/screener/presets
POST /api/screener/presets
DELETE /api/screener/presets/{id}
```

#### Stock Data API
```
GET  /api/stocks/{symbol}
GET  /api/stocks/{symbol}/price-history?fromDate=&toDate=
GET  /api/stocks/{symbol}/fundamentals
GET  /api/stocks/{symbol}/indicators?days=30
GET  /api/stocks/search?query={term}
```

#### Market Data API
```
GET  /api/market/quotes?symbols=AAPL,MSFT,GOOGL
GET  /api/market/status
```

#### Watchlist API
```
GET    /api/watchlists
POST   /api/watchlists
PUT    /api/watchlists/{id}
DELETE /api/watchlists/{id}
POST   /api/watchlists/{id}/items
DELETE /api/watchlists/{id}/items/{stockId}
```

### 4.3 SignalR Hub

```
Hub: /hubs/market-data

Methods:
  - Subscribe(symbols[])    // Subscribe to price updates
  - Unsubscribe(symbols[])  // Unsubscribe
  - GetConnectionStatus()   // Check connection state

Events:
  - OnPriceUpdate(stockId, price, change, changePercent)
  - OnMarketStatusChange(status)
  - OnConnectionStateChanged(state)
```

---

## 5. User Interface Specification

### 5.1 Page Structure

#### Page 1: Screener (Home)
```
┌─────────────────────────────────────────────────────────────────────┐
│  [Logo] Stock Screener          [Search]    [Status] [Settings]    │
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────┐ ┌─────────────────────┐ │
│ │           Filter Panel                  │ │    Results Grid     │ │
│ │                                         │ │                     │ │
│ │ Price: [____] - [____]                  │ │ Symbol | Price |    │ │
│ │ Market Cap: [▼ Large Cap          ]     │ │ Mkt Cap | P/E |     │ │
│ │ Sector:   [▼ All Sectors         ]      │ │ RSI | Div Yield |   │ │
│ │                                         │ │                     │ │
│ │ Valuation:                              │ │ [Row 1]             │ │
│ │   P/E: [____] - [____]                  │ │ [Row 2]             │ │
│ │   P/B: [____] - [____]                  │ │ [Row 3]             │ │
│ │                                         │ │ ...                 │ │
│ │ Technical:                              │ │                     │ │
│ │   RSI: [▼ Overbought (<30)       ]      │ │                     │ │
│ │                                         │ │                     │ │
│ │ [Apply Filters] [Reset] [Save Preset]   │ │ [Export CSV]        │ │
│ └─────────────────────────────────────────┘ └─────────────────────┘ │
├─────────────────────────────────────────────────────────────────────┤
│ Page 1 of 20  [< Prev] [Next >]          Showing 1-50 of 1,000      │
└─────────────────────────────────────────────────────────────────────┘
```

#### Page 2: Stock Detail
```
┌─────────────────────────────────────────────────────────────────────┐
│  [← Back]  AAPL - Apple Inc.                    [Add to Watchlist]  │
├─────────────────────────────────────────────────────────────────────┤
│ ┌─────────────────────────────────────────┐ ┌─────────────────────┐ │
│ │           Candlestick Chart             │ │   Key Statistics    │ │
│ │                                         │ │                     │ │
│ │     [Interactive TradingView-style      │ │ Price: $178.50      │ │
│ │      chart with indicators]             │ │ Change: +2.35%      │ │
│ │                                         │ │ Mkt Cap: $2.8T      │ │
│ │                                         │ │ P/E: 28.5           │ │
│ │ [1D][1W][1M][3M][6M][1Y][5Y][MAX]       │ │ 52W Range: $150-185 │ │
│ └─────────────────────────────────────────┘ └─────────────────────┘ │
│ ┌───────────────────────────────────────────────────────────────────┐
│ │ Fundamentals                          Technical Indicators         │
│ │ ROE: 150%    Div Yield: 0.5%           RSI: 65 (Neutral)          │
│ │ Debt/Eq: 1.2 Revenue Growth: 8%        SMA50: $175 (Above)        │
│ └───────────────────────────────────────────────────────────────────┘
└─────────────────────────────────────────────────────────────────────┘
```

### 5.2 UI Components

| Component | Technology | Purpose |
|-----------|------------|---------|
| DataGrid | Syncfusion Blazor DataGrid | Stock screener results with virtual scrolling |
| Charts | Syncfusion Blazor Charts | Candlestick and line charts |
| Filters | Syncfusion Blazor Dropdowns + Inputs | Filter controls |
| Dialogs | Syncfusion Blazor Dialog | Modals for confirmations, forms |
| Notifications | Syncfusion Blazor Toast | User feedback messages |

### 5.3 Design System

**Color Palette:**
- Primary: `#0D47A1` (Deep Blue)
- Secondary: `#1565C0` (Medium Blue)
- Accent: `#00B0FF` (Light Blue)
- Success/Up: `#4CAF50` (Green)
- Error/Down: `#F44336` (Red)
- Background: `#FAFAFA`
- Surface: `#FFFFFF`
- Text Primary: `#212121`
- Text Secondary: `#757575`
- Border: `#E0E0E0`

**Typography:**
- Font Family: Segoe UI, system-ui, sans-serif
- Headings: 24px (H1), 20px (H2), 16px (H3)
- Body: 14px
- Caption: 12px
- Monospace (numbers): Cascadia Code, Consolas

**Spacing System:**
- Base unit: 4px
- Spacing scale: 4, 8, 12, 16, 24, 32, 48px

---

## 6. Non-Functional Requirements

### 6.1 Performance
| Metric | Target |
|--------|--------|
| Initial page load | < 3 seconds |
| Screener query response | < 2 seconds |
| Price update latency | < 500ms |
| Chart render time | < 1 second |

### 6.2 Scalability
- Support 10,000+ stocks in database
- Handle 1,000 concurrent users
- SignalR hub support for 5,000 simultaneous connections

### 6.3 Security
- API authentication via JWT tokens
- Role-based authorization (Viewer, Editor, Admin)
- HTTPS enforced in production
- Input validation on all endpoints

### 6.4 Data Freshness
- Price data: Real-time during market hours, EOD after close
- Fundamentals: Daily update
- Technical indicators: Calculated nightly after market close

---

## 7. Implementation Phases

### Phase 1: Foundation (MVP)
- [ ] SQL Server database schema setup
- [ ] Entity Framework Core models and DbContext
- [ ] Basic Stock CRUD API
- [ ] Blazor project setup with Syncfusion
- [ ] Stock search functionality
- [ ] Basic filter UI and API integration

### Phase 2: Core Screener
- [ ] Advanced filter panel UI
- [ ] Filter API implementation
- [ ] Paginated results grid
- [ ] Sort functionality
- [ ] Save/load filter presets

### Phase 3: Charts & Visualization
- [ ] Candlestick chart component
- [ ] Price history API
- [ ] Technical indicators API
- [ ] Indicator overlays on charts

### Phase 4: Real-time Data
- [ ] SignalR hub setup
- [ ] WebSocket connection management
- [ ] Live price updates in UI
- [ ] Connection status indicator

### Phase 5: Watchlist & Polish
- [ ] Watchlist CRUD operations
- [ ] Add to watchlist from screener
- [ ] Watchlist quick filter
- [ ] Export to CSV
- [ ] Performance optimization

---

## 8. External Data Sources

### Primary Data Provider: Finnhub
| Data Type | Endpoint | Rate Limit |
|-----------|----------|------------|
| Stock Quotes | `/quote` | 60 calls/min |
| Company Profile | `/stock/profile2` | 60 calls/min |
| Financials | `/stock/metric` | 60 calls/min |
| Price History | `/stock/candle` | 60 calls/min |

### Alternative: Alpha Vantage
| Data Type | Endpoint | Rate Limit |
|-----------|----------|------------|
| Global Quote | `/GLOBAL_QUOTE` | 25/day (free) |
| Income Statement | `/INCOME_STATEMENT` | 25/day (free) |
| Technical Indicators | `/RSI`, `/MACD`, etc. | 25/day (free) |

---

## 9. Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| API rate limiting | High | Implement caching, respect limits, use multiple providers |
| Data accuracy | High | Validate data, show data freshness timestamps |
| Performance at scale | Medium | Optimize queries, implement pagination, use virtual scrolling |
| External API downtime | Medium | Cache last known values, implement retry logic |
| Cost of real-time data | Medium | Start with delayed data, upgrade as needed |

---

## 10. Future Enhancements (Post-MVP)

- [ ] Fundamental data visualization (financial statements)
- [ ] News aggregation and sentiment analysis
- [ ] Portfolio tracking with P&L calculations
- [ ] Alert system (price thresholds, indicator crossovers)
- [ ] Mobile responsive design
- [ ] Multi-language support

---

## Appendix A: Glossary

| Term | Definition |
|------|------------|
| RSI | Relative Strength Index - momentum oscillator (0-100) |
| MACD | Moving Average Convergence Divergence - trend-following indicator |
| SMA | Simple Moving Average |
| P/E Ratio | Price-to-Earnings ratio |
| Market Cap | Total market value of outstanding shares |
| Overbought | RSI > 70, potential pullback expected |
| Oversold | RSI < 30, potential bounce expected |
| Golden Cross | SMA 50 crosses above SMA 200 - bullish signal |
| Death Cross | SMA 50 crosses below SMA 200 - bearish signal |

---

## Appendix B: Reference Data

### Market Cap Categories
| Category | Range (USD) |
|----------|-------------|
| Mega Cap | > $200 billion |
| Large Cap | $10 billion - $200 billion |
| Mid Cap | $2 billion - $10 billion |
| Small Cap | $300 million - $2 billion |
| Micro Cap | < $300 million |

### RSI Interpretation
| Value | Interpretation |
|-------|----------------|
| > 70 | Overbought |
| 50-70 | Neutral to Bullish |
| 30-50 | Neutral to Bearish |
| < 30 | Oversold |

---

*Document Version: 1.0 | Last Updated: January 2025*