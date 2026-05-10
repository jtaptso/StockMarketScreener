using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockScreener.Application.DTOs;
using StockScreener.Application.Interfaces;

namespace StockScreener.API.Controllers;

[ApiController]
[Route("api/screener")]
[Authorize]
public class ScreenerController(IScreenerService screenerService) : ControllerBase
{
    // GET /api/screener/filter
    [HttpGet("filter")]
    [ProducesResponseType(typeof(ScreenerResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Filter([FromQuery] ScreenerFilterDto filter, CancellationToken ct)
    {
        var result = await screenerService.FilterAsync(filter, ct);
        return Ok(result);
    }

    // GET /api/screener/presets
    [HttpGet("presets")]
    [ProducesResponseType(typeof(IEnumerable<FilterPresetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPresets(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var presets = await screenerService.GetPresetsAsync(userId, ct);
        return Ok(presets);
    }

    // POST /api/screener/presets
    [HttpPost("presets")]
    [ProducesResponseType(typeof(FilterPresetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SavePreset([FromBody] SavePresetRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var preset = await screenerService.SavePresetAsync(userId, request.Name, request.Description, request.Filter, ct);
        return CreatedAtAction(nameof(GetPresets), preset);
    }

    // DELETE /api/screener/presets/{id}
    [HttpDelete("presets/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePreset(Guid id, CancellationToken ct)
    {
        await screenerService.DeletePresetAsync(id, ct);
        return NoContent();
    }
}

public record SavePresetRequest(string Name, string? Description, ScreenerFilterDto Filter);
