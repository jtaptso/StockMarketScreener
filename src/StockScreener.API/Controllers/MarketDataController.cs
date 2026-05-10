using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockScreener.Application.Interfaces;

namespace StockScreener.API.Controllers;

[ApiController]
[Route("api/market-data")]
[Authorize]
public class MarketDataController(IMarketDataService marketDataService) : ControllerBase
{
    // POST /api/market-data/sync
    // Manually trigger a full sync of all tracked stocks.
    [HttpPost("sync")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult Sync(CancellationToken ct)
    {
        // Fire and forget — the sync runs in the background.
        _ = marketDataService.SyncAllAsync(ct);
        return Accepted();
    }

    // POST /api/market-data/sync/{symbol}
    // Manually trigger a sync for a single symbol.
    [HttpPost("sync/{symbol}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult SyncSymbol(string symbol, CancellationToken ct)
    {
        _ = marketDataService.SyncSymbolAsync(symbol, ct);
        return Accepted();
    }
}
