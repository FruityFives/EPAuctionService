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
    /// Opretter en ny auktion.
    /// </summary>
    /// <param name="auction">Auktionsobjektet der skal oprettes.</param>
    /// <returns>Returnerer den oprettede auktion og dens placering.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
    {
        _logger.LogInformation("CreateAuction called with Auction: {@Auction}", auction);

        if (auction == null)
        {
            _logger.LogWarning("CreateAuction modtog null Auction");
            return BadRequest("Auction kan ikke være null");
        }

        try
        {
            var result = await _auctionService.CreateAuction(auction);
            _logger.LogInformation("Auction oprettet med ID: {AuctionId}", result.AuctionId);
            return CreatedAtAction(nameof(GetAuctionById), new { id = result.AuctionId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl opstod under oprettelse af auktion");
            return StatusCode(500, "Intern serverfejl");
        }
    }

    /// <summary>
    /// Henter en auktion ud fra det angivne ID.
    /// </summary>
    /// <param name="id">ID'et på den ønskede auktion.</param>
    /// <returns>Returnerer auktionen, hvis den findes.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        _logger.LogInformation("GetAuctionById kaldt med ID: {Id}", id);

        var result = await _auctionService.GetAuctionById(id);
        if (result == null)
        {
            _logger.LogWarning("Ingen auktion fundet med ID: {Id}", id);
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Sletter en auktion med det angivne ID.
    /// </summary>
    /// <param name="id">ID'et på auktionen der skal slettes.</param>
    /// <returns>Returnerer NoContent, hvis den blev slettet. Ellers NotFound.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        _logger.LogInformation("DeleteAuction kaldt med ID: {Id}", id);

        var result = await _auctionService.DeleteAuction(id);
        if (!result)
        {
            _logger.LogWarning("Sletning fejlede. Auktion ikke fundet med ID: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Auktion slettet med ID: {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Opdaterer status på en auktion.
    /// </summary>
    /// <param name="id">ID på auktionen der skal opdateres.</param>
    /// <param name="status">Den nye status for auktionen.</param>
    /// <returns>Returnerer den opdaterede auktion hvis succesfuldt, ellers NotFound.</returns>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAuctionStatus(Guid id, [FromBody] AuctionStatus status)
    {
        _logger.LogInformation("UpdateAuctionStatus kaldt for auktion ID: {Id} med status: {Status}", id, status);

        var updated = await _auctionService.UpdateAuctionStatus(id, status);
        if (updated == null)
        {
            _logger.LogWarning("Opdatering af status fejlede. Auktion ikke fundet med ID: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Auktionsstatus opdateret for ID: {Id} til {Status}", id, status);
        return Ok(updated);
    }
}

