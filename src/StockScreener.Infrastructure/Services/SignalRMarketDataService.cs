using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockScreener.Application.Interfaces;
using StockScreener.Domain.Interfaces.Repositories;

namespace StockScreener.Infrastructure.Services;

/// <summary>
/// Hosted background service that polls the stock repository for current prices
/// and broadcasts updates to connected SignalR clients via <see cref="IPriceUpdateBroadcaster"/>.
/// </summary>
public class SignalRMarketDataService(
    IServiceScopeFactory scopeFactory,
    IPriceUpdateBroadcaster broadcaster,
    ILogger<SignalRMarketDataService> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(15);

    // Tracks the last known price per symbol to only broadcast actual changes.
    private readonly Dictionary<string, decimal> _lastPrices = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("SignalRMarketDataService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await PollAndBroadcastAsync(stoppingToken);
            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task PollAndBroadcastAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var stockRepo = scope.ServiceProvider.GetRequiredService<IStockRepository>();

            var stocks = await stockRepo.GetAllAsync(cancellationToken);

            foreach (var stock in stocks)
            {
                if (stock.CurrentPrice is null) continue;

                var price = stock.CurrentPrice.Value;
                var hasPrev = _lastPrices.TryGetValue(stock.Symbol, out var prev);

                // Only broadcast when price has actually changed (or on first observation).
                if (!hasPrev || prev != price)
                {
                    var change = hasPrev ? price - prev : 0m;
                    _lastPrices[stock.Symbol] = price;

                    await broadcaster.BroadcastPriceUpdateAsync(
                        stock.Symbol, price, change, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SignalRMarketDataService: poll failed");
        }
    }
}
