# Bourse - Stock Market Screener

A modern stock market screener application built with .NET 10, Blazor WebAssembly, and SQL Server. Enables investors to filter and analyze publicly traded securities based on fundamental and technical criteria.

## 🎯 Overview

Bourse provides:
- **Advanced Stock Screening** - Filter stocks by 20+ criteria (P/E, RSI, market cap, sector, etc.)
- **Real-time Price Updates** - Live market data via SignalR WebSocket connections
- **Interactive Charts** - Candlestick charts with technical indicator overlays
- **Watchlist Management** - Create and track custom stock watchlists
- **Technical Analysis** - Built-in indicators (RSI, MACD, SMA, Bollinger Bands, ATR)

## 🛠 Tech Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | Blazor WebAssembly (.NET 10) |
| **UI Components** | Syncfusion Blazor UI Suite |
| **API** | ASP.NET Core Web API (.NET 10) |
| **Real-time** | SignalR |
| **Database** | SQL Server 2022 |
| **ORM** | Entity Framework Core 10 |
| **Architecture** | Clean Architecture |

## 📁 Project Structure

```
src/
├── Bourse.Domain/           # Core business logic (no external dependencies)
│   ├── Entities/            # Stock, PriceHistory, Fundamentals, etc.
│   ├── Enums/               # Exchange, MarketCapCategory, SortOrder
│   ├── ValueObjects/        # Money, DateRange, Percentage
│   ├── Interfaces/          # Repository & service interfaces
│   └── Services/            # ScreenerEngine, IndicatorCalculator
│
├── Bourse.Application/      # Application layer (depends only on Domain)
│   ├── DTOs/                # Data transfer objects
│   ├── Interfaces/          # Use case service interfaces
│   └── Services/            # ScreenerService, StockService, WatchlistService
│
├── Bourse.Infrastructure/   # External concerns (depends on Domain + Application)
│   ├── Persistence/         # EF Core DbContext, repositories
│   ├── External/            # Finnhub/AlphaVantage API clients
│   ├── Services/            # Caching, SignalR, background sync
│   └── Mapping/             # AutoMapper profiles
│
├── Bourse.Presentation/     # Blazor WebAssembly UI
│   ├── Pages/               # Screener, StockDetail, Watchlist, Settings
│   ├── Components/          # Reusable UI components
│   ├── ViewModels/          # MVVM view models
│   └── Services/            # API client, SignalR service
│
└── Bourse.Tests/            # Unit and integration tests
    ├── Domain/              # Domain service tests
    ├── Application/         # Application service tests
    └── Infrastructure/      # Repository tests
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server 2022](https://www.microsoft.com/sql-server/sql-server-downloads) (or SQL Server Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension
- [Finnhub API Key](https://finnhub.io/) (free tier available)
- [Alpha Vantage API Key](https://www.alphavantage.co/) (backup, free tier available)

### Getting Your API Keys

**Finnhub (Primary)**
1. Visit [finnhub.io](https://finnhub.io/)
2. Sign up for a free account
3. Navigate to your dashboard to copy your API key
4. Free tier: 60 calls/minute

**Alpha Vantage (Backup)**
1. Visit [alphavantage.co](https://www.alphavantage.co/)
2. Sign up for a free API key
3. Free tier: 25 calls/day

### Configuration

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Bourse
   ```

2. **Configure database connection**
   
   Copy the template and configure your settings:
   ```bash
   cp src/Bourse.Presentation/appsettings.json src/Bourse.Presentation/appsettings.Development.json
   ```
   
   Edit `src/Bourse.Presentation/appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=BourseDb;Trusted_Connection=True;TrustServerCertificate=True"
     },
     "ExternalApis": {
       "Finnhub": {
         "ApiKey": "YOUR_FINNHUB_API_KEY"
       }
     }
   }
   ```

### Database Setup

1. **Create the database**
   ```bash
   cd src/Bourse.Presentation
   dotnet ef database create --project ../Bourse.Infrastructure --startup-project .
   ```

2. **Run migrations**
   ```bash
   dotnet ef migrations add InitialCreate --project ../Bourse.Infrastructure --startup-project .
   dotnet ef database update --project ../Bourse.Infrastructure --startup-project .
   ```

### Running the Application

**Development Mode:**
```bash
cd src/Bourse.Presentation
dotnet run
```

The API will be available at `https://localhost:5001` and the Blazor UI at `https://localhost:5002`.

**Using Docker Compose:**
```bash
docker-compose up -d
```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment (Development/Staging/Production) | Development |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | - |
| `FinnhubApiKey` | Finnhub API key for market data | - |
| `AlphaVantageApiKey` | Alpha Vantage API key (backup data source) | - |

## 📐 Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│              (Blazor WebAssembly - UI)                       │
├─────────────────────────────────────────────────────────────┤
│                    APPLICATION LAYER                         │
│         (Use Cases, DTOs, Service Interfaces)                │
├─────────────────────────────────────────────────────────────┤
│                       DOMAIN LAYER                           │
│        (Entities, Business Rules - No Dependencies)          │
├─────────────────────────────────────────────────────────────┤
│                   INFRASTRUCTURE LAYER                       │
│      (EF Core, API Clients, External Services)               │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Rules

| Layer | Can Depend On | Cannot Depend On |
|-------|---------------|------------------|
| Domain | None | Application, Infrastructure, Presentation |
| Application | Domain | Infrastructure, Presentation |
| Infrastructure | Domain, Application | Presentation |
| Presentation | Domain (via Application) | - |

## 🔌 API Endpoints

### Screener API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/screener/filter` | Filter stocks with multiple criteria |
| `GET` | `/api/v1/screener/presets` | Get saved filter presets |
| `POST` | `/api/v1/screener/presets` | Create new preset |
| `DELETE` | `/api/v1/screener/presets/{id}` | Delete preset |

### Stock Data API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/stocks/{symbol}` | Get stock details |
| `GET` | `/api/v1/stocks/{symbol}/price-history` | Get historical prices |
| `GET` | `/api/v1/stocks/{symbol}/fundamentals` | Get fundamental data |
| `GET` | `/api/v1/stocks/{symbol}/indicators` | Get technical indicators |
| `GET` | `/api/v1/stocks/search?query={term}` | Search stocks |

### Market Data API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/market/quotes?symbols=AAPL,MSFT` | Get real-time quotes |
| `GET` | `/api/v1/market/status` | Get market status |

### Watchlist API

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/v1/watchlists` | Get all watchlists |
| `POST` | `/api/v1/watchlists` | Create watchlist |
| `GET` | `/api/v1/watchlists/{id}` | Get watchlist with items |
| `PUT` | `/api/v1/watchlists/{id}` | Update watchlist |
| `DELETE` | `/api/v1/watchlists/{id}` | Delete watchlist |
| `POST` | `/api/v1/watchlists/{id}/items` | Add stock to watchlist |
| `DELETE` | `/api/v1/watchlists/{id}/items/{symbol}` | Remove stock |

### SignalR Hub

**Hub Path:** `/hubs/market-data`

**Client Methods:**
- `SubscribeToSymbols(string[] symbols)` - Subscribe to price updates
- `UnsubscribeFromSymbols(string[] symbols)` - Unsubscribe
- `GetConnectionState()` - Get connection status

**Server Events:**
- `OnPriceUpdate` - Real-time price change
- `OnMarketStatusChange` - Market open/close status
- `OnConnectionStateChanged` - Connection state updates

## 📊 Database Schema

### Core Tables

- **Stocks** - Master stock data (symbol, company name, sector, market cap)
- **PriceHistory** - Historical OHLCV data
- **Fundamentals** - Financial metrics (P/E, P/B, ROE, etc.)
- **TechnicalIndicators** - Calculated indicators (RSI, SMA, MACD, etc.)
- **Watchlists** - User watchlists
- **WatchlistItems** - Stocks in watchlists
- **FilterPresets** - Saved filter configurations

### Key Indexes

- `IX_Stocks_Symbol` - Fast symbol lookups
- `IX_PriceHistory_StockId_Date` - Historical data queries
- `IX_TechnicalIndicators_StockId_Date` - Technical indicator queries

## 🔧 Development

### Running Tests

```bash
dotnet test
```

### Code Style

This project uses:
- C# 13/.NET 10 features (Primary Constructors, Collection Expressions)
- Clean Architecture pattern
- Repository pattern for data access
- FluentValidation for input validation

### Building

```bash
# Build all projects
dotnet build

# Build release
dotnet build --configuration Release

# Publish for deployment
dotnet publish --configuration Release --output ./publish
```

## 📦 External Data Sources

| Provider | Purpose | Rate Limits |
|----------|---------|-------------|
| **Finnhub** | Primary real-time data | 60 calls/min (free tier) |
| **Alpha Vantage** | Backup/fundamental data | 25 calls/day (free tier) |

## 🔒 Security

- JWT Bearer token authentication
- Role-based authorization (Viewer, Editor, Admin)
- Rate limiting on API endpoints
- Input validation with FluentValidation
- HTTPS enforced in production
- API keys stored in environment variables/Key Vault

## 🚢 Deployment

### Azure App Service

Recommended deployment target for production:
- Use deployment slots for zero-downtime updates
- Enable Application Insights for monitoring
- Configure auto-scaling based on CPU/memory

### Docker

```bash
# Build image
docker build -t bourse:latest .

# Run container
docker run -d -p 5000:8080 --env-file .env bourse:latest
```

### Kubernetes

Manifest files available in `/deploy/kubernetes/`:
- Deployment with horizontal pod autoscaling
- Service with load balancer
- ConfigMaps and Secrets

## 📝 License

[MIT License](LICENSE)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📚 Documentation

- [Product Requirement Document (PRD)](PRD-StockMarketScreener.md)
- [Technical Design Document](Design-Document-StockMarketScreener.md)

---

**Built with Clean Architecture principles for maintainability, testability, and scalability.**