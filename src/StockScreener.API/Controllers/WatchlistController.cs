using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockScreener.Application.DTOs;
using StockScreener.Application.Interfaces;

namespace StockScreener.API.Controllers;

[ApiController]
[Route("api/watchlists")]
[Authorize]
public class WatchlistController(IWatchlistService watchlistService) : ControllerBase
{
    private string UserId =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    // GET /api/watchlists
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<WatchlistDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var watchlists = await watchlistService.GetByUserIdAsync(UserId, ct);
        return Ok(watchlists);
    }

    // POST /api/watchlists
    [HttpPost]
    [ProducesResponseType(typeof(WatchlistDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateWatchlistRequest request, CancellationToken ct)
    {
        var watchlist = await watchlistService.CreateAsync(UserId, request.Name, request.Description, ct);
        return CreatedAtAction(nameof(GetById), new { id = watchlist.Id }, watchlist);
    }

    // GET /api/watchlists/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WatchlistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var watchlist = await watchlistService.GetByIdAsync(id, ct);
        return watchlist is null ? NotFound() : Ok(watchlist);
    }

    // DELETE /api/watchlists/{id}
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await watchlistService.DeleteAsync(id, ct);
        return NoContent();
    }

    // POST /api/watchlists/{id}/stocks
    [HttpPost("{id:guid}/stocks")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddStock(
        Guid id,
        [FromBody] AddStockRequest request,
        CancellationToken ct)
    {
        await watchlistService.AddStockAsync(id, request.Symbol, request.SharesOwned, request.CostBasis, ct);
        return NoContent();
    }

    // DELETE /api/watchlists/{id}/stocks/{symbol}
    [HttpDelete("{id:guid}/stocks/{symbol}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveStock(Guid id, string symbol, CancellationToken ct)
    {
        await watchlistService.RemoveStockAsync(id, symbol, ct);
        return NoContent();
    }
}

public record CreateWatchlistRequest(string Name, string? Description);
public record AddStockRequest(string Symbol, decimal? SharesOwned, decimal? CostBasis);
