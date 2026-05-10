using Microsoft.AspNetCore.SignalR;

namespace StockScreener.API.Hubs;

/// <summary>
/// SignalR hub for real-time market data streaming.
/// Clients call <see cref="SubscribeToSymbols"/> to opt into price updates
/// for specific symbols. The server broadcasts <c>PriceUpdate</c> events.
/// </summary>
public class MarketDataHub : Hub
{
    /// <summary>
    /// Adds the caller to per-symbol SignalR groups so they receive targeted updates.
    /// </summary>
    public async Task SubscribeToSymbols(string[] symbols)
    {
        foreach (var symbol in symbols)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, symbol.ToUpperInvariant());
        }
    }

    /// <summary>
    /// Removes the caller from the given symbol groups.
    /// </summary>
    public async Task UnsubscribeFromSymbols(string[] symbols)
    {
        foreach (var symbol in symbols)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, symbol.ToUpperInvariant());
        }
    }
}
