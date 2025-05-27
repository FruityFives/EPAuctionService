using AuctionServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Controllers;

/// <summary>
/// Controller til håndtering af auktioner.
/// </summary>
[ApiController]
[Route("api/auction")]
public class AuctionController : ControllerBase
{
    private readonly IAuctionService _auctionService;
    private readonly ILogger<AuctionController> _logger;

    public AuctionController(IAuctionService auctionService, ILogger<AuctionController> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
        _logger.LogInformation("AuctionController initialized");
    }

    /// <summary>
    /// Tildeler en auktion til et katalog med en minimumspris.
    /// </summary>
    [HttpPost("{auctionId}/assign-to-catalog")]
    public async Task<IActionResult> AssignAuctionToCatalog(Guid auctionId, [FromQuery] Guid catalogId, [FromQuery] double minPrice)
    {
        var result = await _auctionService.AddAuctionToCatalog(auctionId, catalogId, minPrice);
        if (result == null)
            return NotFound($"Auction with ID {auctionId} not found");

        return Ok(result);
    }

    /// <summary>
    /// Henter en auktion baseret på dens ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        _logger.LogInformation("GetAuctionById called with ID: {Id}", id);

        var result = await _auctionService.GetAuctionById(id);
        if (result == null)
        {
            _logger.LogWarning("Auction not found with ID: {Id}", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Sletter en auktion baseret på dens ID.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        _logger.LogInformation("DeleteAuction called with ID: {Id}", id);

        var result = await _auctionService.DeleteAuction(id);
        if (!result)
        {
            _logger.LogWarning("DeleteAuction failed. Auction not found with ID: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Auction deleted with ID: {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Opdaterer status på en auktion.
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAuctionStatus(Guid id, [FromBody] AuctionStatus status)
    {
        _logger.LogInformation("UpdateAuctionStatus called for Auction ID: {Id} with Status: {Status}", id, status);

        var updated = await _auctionService.UpdateAuctionStatus(id, status);
        if (updated == null)
        {
            _logger.LogWarning("UpdateAuctionStatus failed. Auction not found with ID: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Auction status updated for ID: {Id} to {Status}", id, status);
        return Ok(updated);
    }
}
