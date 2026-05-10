using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockScreener.API.Hubs;
using StockScreener.API.Services;
using StockScreener.Application.Interfaces;
using StockScreener.Application.Services;
using StockScreener.Domain.Interfaces.Repositories;
using StockScreener.Domain.Interfaces.Services;
using StockScreener.Domain.Services;
using StockScreener.Infrastructure.External;
using StockScreener.Infrastructure.Mapping;
using StockScreener.Infrastructure.Persistence;
using StockScreener.Infrastructure.Persistence.Repositories;
using StockScreener.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

// ── Repositories (Scoped) ─────────────────────────────────────────────────────
builder.Services.AddScoped<IStockRepository, StockRepository>();
builder.Services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
builder.Services.AddScoped<IFundamentalsRepository, FundamentalsRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddScoped<IFilterPresetRepository, FilterPresetRepository>();

// ── Domain Services (Singleton — pure stateless logic) ────────────────────────
builder.Services.AddSingleton<IIndicatorCalculator, IndicatorCalculator>();
builder.Services.AddSingleton<IScreenerEngine, ScreenerEngine>();

// ── Application Services (Scoped) ────────────────────────────────────────────
builder.Services.AddScoped<IScreenerService, ScreenerService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IMarketDataService, MarketDataService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();

// ── Caching ───────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICachingService, CachingService>();

// ── External HTTP Clients ─────────────────────────────────────────────────────
builder.Services.AddHttpClient<FinnhubClient>(client =>
{
    client.BaseAddress = new Uri(config["Finnhub:BaseUrl"] ?? "https://finnhub.io/api/v1");
    client.DefaultRequestHeaders.Add("X-Finnhub-Token", config["Finnhub:ApiKey"]);
});

builder.Services.AddHttpClient<AlphaVantageClient>(client =>
{
    client.BaseAddress = new Uri(config["AlphaVantage:BaseUrl"] ?? "https://www.alphavantage.co/query");
});

// ── AutoMapper ────────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();
builder.Services.AddSingleton<IPriceUpdateBroadcaster, SignalRPriceUpdateBroadcaster>();

// ── Background Services ───────────────────────────────────────────────────────
builder.Services.AddHostedService<BackgroundDataSyncService>();
builder.Services.AddHostedService<SignalRMarketDataService>();

// ── JWT Bearer Authentication ─────────────────────────────────────────────────
var jwtKey = config["Jwt:Key"] ?? "dev-secret-key-change-in-production-32chars!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = config["Jwt:Issuer"] ?? "StockScreener.API",
            ValidAudience            = config["Jwt:Audience"] ?? "StockScreener.Client",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        // Allow SignalR to pass JWT via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy
            .WithOrigins(
                config["Cors:BlazorOrigin"] ?? "https://localhost:7200",
                "http://localhost:5200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // required for SignalR
    });
});

// ── Controllers & OpenAPI ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "StockScreener API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Enter your JWT token."
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            []
        }
    });
});

// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// Must be first — catches all exceptions from downstream middleware and controllers.
app.UseMiddleware<StockScreener.API.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StockScreener API v1"));
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MarketDataHub>("/hubs/market-data");

app.Run();

