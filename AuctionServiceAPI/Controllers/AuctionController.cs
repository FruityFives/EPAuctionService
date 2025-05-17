using AuctionServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;   
using System.Diagnostics;

namespace AuctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{
   
    private readonly ILogger<AuctionController> _logger;
    private readonly ICatalogService _catalogService;
    private readonly IAuctionService _auctionService;
    // herhenne
    public AuctionController(ILogger<AuctionController> logger, ICatalogService catalogService, IAuctionService auctionService)
    {
        _logger = logger;
        _catalogService = catalogService;
        _auctionService = auctionService;
    }

    //Endpoint for create auction
    [HttpPost("create")]
    public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
    {
        if (auction == null)
        {
            return BadRequest("Auction cannot be null");
        }

        // Call the service to create the auction
        var createdAuction = await _auctionService.CreateAuction(auction);
        //update the catalog
        var catalog = await _catalogService.GetCatalogById(auction.CatalogId);

        if (createdAuction == null)
        {
            return BadRequest("Failed to create auction");
        }

        return CreatedAtAction(nameof(GetAuctionById), new { id = createdAuction.AuctionId }, createdAuction);
    }

    //Endpoint for get auction by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        var auction = await _auctionService.GetAuctionById(id);

        if (auction == null)
        {
            return NotFound();
        }

        return Ok(auction);
    }

    //Endpoint for delete auction
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid auctionid, Guid catalogid)
    {
        var result = await _auctionService.DeleteAuction(auctionid);
        //update the catalog 
        var catalog = await _catalogService.GetCatalogById(catalogid);
        if (catalog != null)
        {
            catalog.Auctions.RemoveAll(a => a.AuctionId == auctionid);
            await _catalogService.UpdateCatalog(catalog);
        }

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    //Endpoint for update auction status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAuctionStatus(Guid id, [FromBody] AuctionStatus status)
    {
        var updatedAuction = await _auctionService.UpdateAuctionStatus(id, status);

        if (updatedAuction == null)
        {
            return NotFound();
        }

        return Ok(updatedAuction);
    }

    //Endpoint for create bid to auction by id
    [HttpPost("{auctionId}/bid")]
    public async Task<IActionResult> CreateBidToAuctionById(Guid auctionId, [FromBody] BidDTO bid)
    {
        if (bid == null)
        {
            return BadRequest("Bid cannot be null");
        }

        var updatedAuction = await _auctionService.CreateBidToAuctionById(auctionId, bid);

        if (updatedAuction == null)
        {
            return NotFound();
        }

        return Ok(updatedAuction);
    }

    //Endpoint for get active auctions by catalog id
    [HttpGet("{catalogId}/active")]
    public async Task<IActionResult> GetActiveAuctions(Guid catalogId)
    {
        var activeAuctions = await _auctionService.SendActiveAuctions(catalogId, AuctionStatus.Active);

        if (activeAuctions == null || !activeAuctions.Any())
        {
            return NotFound();
        }

        return Ok(activeAuctions);
    }

    //Endpoint for get finished auctions by catalog id
    [HttpGet("{catalogId}/finished")]
    public async Task<IActionResult> GetFinishedAuctions(Guid catalogId)
    {
        var finishedAuctions = await _auctionService.SendActiveAuctions(catalogId, AuctionStatus.Closed);

        if (finishedAuctions == null || !finishedAuctions.Any())
        {
            return NotFound();
        }

        return Ok(finishedAuctions);
    }





}
