using AuctionServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionController : ControllerBase
{
    private readonly IAuctionService _auctionService;
    private readonly ILogger<AuctionController> _logger;

    public AuctionController(IAuctionService auctionService, ILogger<AuctionController> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
    {
        if (auction == null) return BadRequest("Auction cannot be null");
        var result = await _auctionService.CreateAuction(auction);
        return CreatedAtAction(nameof(GetAuctionById), new { id = result.AuctionId }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        var result = await _auctionService.GetAuctionById(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var result = await _auctionService.DeleteAuction(id);
        return result ? NoContent() : NotFound();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAuctionStatus(Guid id, [FromBody] AuctionStatus status)
    {
        var updated = await _auctionService.UpdateAuctionStatus(id, status);
        return updated == null ? NotFound() : Ok(updated);
    }

}