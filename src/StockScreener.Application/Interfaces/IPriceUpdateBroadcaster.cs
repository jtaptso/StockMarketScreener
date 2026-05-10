namespace StockScreener.Application.Interfaces;

/// <summary>
/// Port for broadcasting real-time price updates to connected clients.
/// Implemented in the API layer using <c>IHubContext&lt;MarketDataHub&gt;</c>.
/// </summary>
public interface IPriceUpdateBroadcaster
{
    /// <summary>Pushes a price update event to all subscribed clients.</summary>
    Task BroadcastPriceUpdateAsync(
        string symbol,
        decimal price,
        decimal change,
        CancellationToken cancellationToken = default);
}
