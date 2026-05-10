using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockScreener.Application.DTOs;
using StockScreener.Application.Interfaces;

namespace StockScreener.API.Controllers;

[ApiController]
[Route("api/stocks")]
[Authorize]
public class StocksController(IStockService stockService) : ControllerBase
{
    // GET /api/stocks/{symbol}
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySymbol(string symbol, CancellationToken ct)
    {
        var stock = await stockService.GetBySymbolAsync(symbol, ct);
        return stock is null ? NotFound() : Ok(stock);
    }

    // GET /api/stocks/{symbol}/price-history?from=2025-01-01&to=2025-12-31
    [HttpGet("{symbol}/price-history")]
    [ProducesResponseType(typeof(IEnumerable<PriceHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPriceHistory(
        string symbol,
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        if (from > to)
            return BadRequest("'from' must be before or equal to 'to'.");

        var history = await stockService.GetPriceHistoryAsync(symbol, from, to, ct);
        return Ok(history);
    }

    // GET /api/stocks/{symbol}/fundamentals
    [HttpGet("{symbol}/fundamentals")]
    [ProducesResponseType(typeof(FundamentalsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFundamentals(string symbol, CancellationToken ct)
    {
        var fundamentals = await stockService.GetFundamentalsAsync(symbol, ct);
        return fundamentals is null ? NotFound() : Ok(fundamentals);
    }

    // GET /api/stocks/{symbol}/indicators
    // TechnicalIndicators are returned as part of the StockDto for now;
    // a dedicated endpoint is reserved for Phase 8 / future expansion.
    [HttpGet("{symbol}/indicators")]
    [ProducesResponseType(typeof(StockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIndicators(string symbol, CancellationToken ct)
    {
        var stock = await stockService.GetBySymbolAsync(symbol, ct);
        return stock is null ? NotFound() : Ok(stock);
    }
}
