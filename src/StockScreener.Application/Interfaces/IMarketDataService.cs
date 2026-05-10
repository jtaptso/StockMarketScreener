namespace StockScreener.Application.Interfaces;

public interface IMarketDataService
{
    /// <summary>
    /// Triggers a full sync of price and fundamental data for all tracked symbols.
    /// </summary>
    Task SyncAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs price and fundamental data for a single symbol.
    /// </summary>
    Task SyncSymbolAsync(string symbol, CancellationToken cancellationToken = default);
}
