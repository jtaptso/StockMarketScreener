using Microsoft.AspNetCore.SignalR;
using StockScreener.Application.Interfaces;
using StockScreener.API.Hubs;

namespace StockScreener.API.Services;

/// <summary>
/// Implements <see cref="IPriceUpdateBroadcaster"/> using SignalR.
/// Registered as a singleton in the API composition root so it can be injected
/// into the Infrastructure background service without a circular project reference.
/// Broadcasts to the group named after each symbol so only subscribed clients receive updates.
/// </summary>
public class SignalRPriceUpdateBroadcaster(IHubContext<MarketDataHub> hubContext)
    : IPriceUpdateBroadcaster
{
    public Task BroadcastPriceUpdateAsync(
        string symbol,
        decimal price,
        decimal change,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients
            .Group(symbol.ToUpperInvariant())
            .SendAsync("PriceUpdate", new { symbol, price, change }, cancellationToken);
    }
}
