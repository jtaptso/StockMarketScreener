using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StockScreener.Application.Interfaces;

namespace StockScreener.Infrastructure.Services;

/// <summary>
/// Hosted background service that triggers a full market data sync once per day.
/// Uses <see cref="IServiceScopeFactory"/> because <see cref="IMarketDataService"/>
/// is registered as a scoped service.
/// </summary>
public class BackgroundDataSyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundDataSyncService> logger) : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BackgroundDataSyncService starting.");

        // Run an initial sync shortly after startup, then repeat daily.
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncAsync(stoppingToken);
            await Task.Delay(SyncInterval, stoppingToken);
        }
    }

    private async Task SyncAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BackgroundDataSyncService: starting full sync at {Time}", DateTime.UtcNow);
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var marketDataService = scope.ServiceProvider.GetRequiredService<IMarketDataService>();
            await marketDataService.SyncAllAsync(cancellationToken);
            logger.LogInformation("BackgroundDataSyncService: sync completed at {Time}", DateTime.UtcNow);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown — do not log as error.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BackgroundDataSyncService: sync failed");
        }
    }
}
