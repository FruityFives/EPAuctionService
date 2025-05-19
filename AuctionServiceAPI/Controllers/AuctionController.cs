using AuctionServiceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Linq;
using System.Threading.Tasks;

namespace AuctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{
    private readonly ILogger<AuctionController> _logger;
    private readonly ICatalogService _catalogService;
    private readonly IAuctionService _auctionService;

    private readonly string _ipaddr;

    public AuctionController(ILogger<AuctionController> logger, ICatalogService catalogService, IAuctionService auctionService)
    {
        _logger = logger;
        _catalogService = catalogService;
        _auctionService = auctionService;

        var hostName = System.Net.Dns.GetHostName();
        var ips = System.Net.Dns.GetHostAddresses(hostName);
        _ipaddr = ips.First().MapToIPv4().ToString();
        _logger.LogInformation($"XYZ Service responding from {_ipaddr}");
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateAuction([FromBody] Auction auction)
    {
        _logger.LogInformation("CreateAuction called with Auction: {@Auction}", auction);

        if (auction == null)
        {
            _logger.LogWarning("CreateAuction failed: Auction is null");
            return BadRequest("Auction cannot be null");
        }

        var catalogexist = await _catalogService.GetCatalogById(auction.CatalogId);
        if (catalogexist == null)
        {
            _logger.LogWarning("CreateAuction failed: Catalog with Id {CatalogId} does not exist", auction.CatalogId);
            return BadRequest("Catalog does not exist");
        }

        var createdAuction = await _auctionService.CreateAuction(auction);

        if (createdAuction == null)
        {
            _logger.LogError("CreateAuction failed: Could not create auction {@Auction}", auction);
            return BadRequest("Failed to create auction");
        }

        _logger.LogInformation("Auction created successfully with Id {AuctionId}", createdAuction.AuctionId);

        return CreatedAtAction(nameof(GetAuctionById), new { id = createdAuction.AuctionId }, createdAuction);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        _logger.LogInformation("GetAuctionById called with Id: {Id}", id);

        var auction = await _auctionService.GetAuctionById(id);

        if (auction == null)
        {
            _logger.LogWarning("GetAuctionById: Auction with Id {Id} not found", id);
            return NotFound();
        }

        return Ok(auction);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid auctionid, Guid catalogid)
    {
        _logger.LogInformation("DeleteAuction called with AuctionId: {AuctionId}, CatalogId: {CatalogId}", auctionid, catalogid);

        var result = await _auctionService.DeleteAuction(auctionid);
        var catalog = await _catalogService.GetCatalogById(catalogid);

        if (catalog != null)
        {
            catalog.Auctions.RemoveAll(a => a.AuctionId == auctionid);
            await _catalogService.UpdateCatalog(catalog);
            _logger.LogInformation("Catalog updated after deleting auction {AuctionId}", auctionid);
        }

        if (!result)
        {
            _logger.LogWarning("DeleteAuction failed: Auction with Id {AuctionId} not found", auctionid);
            return NotFound();
        }

        _logger.LogInformation("Auction with Id {AuctionId} deleted successfully", auctionid);
        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAuctionStatus(Guid id, [FromBody] AuctionStatus status)
    {
        _logger.LogInformation("UpdateAuctionStatus called for AuctionId: {Id} with status {Status}", id, status);

        var updatedAuction = await _auctionService.UpdateAuctionStatus(id, status);

        if (updatedAuction == null)
        {
            _logger.LogWarning("UpdateAuctionStatus failed: Auction with Id {Id} not found", id);
            return NotFound();
        }

        _logger.LogInformation("Auction status updated for AuctionId {Id} to {Status}", id, status);
        return Ok(updatedAuction);
    }

    [HttpPost("{auctionId}/bid")]
    public async Task<IActionResult> CreateBidToAuctionById([FromBody] BidDTO bid)
    {


        Console.WriteLine("CreateBidToAuctionById called");

    if (bid == null)
        return BadRequest("Bid cannot be null");

    try
    {
        var updatedAuction = await _auctionService.CreateBidToAuctionById(bid);

        _logger.LogInformation("CreateBidToAuctionById called for AuctionId: {AuctionId} with bid {@Bid}", auctionId, bid);
        Console.WriteLine("CreateBidToAuctionById called");


    if (bid == null)
        return BadRequest("Bid cannot be null");


        _logger.LogInformation("Bid created successfully on AuctionId {AuctionId} with BidId {BidId}", auctionId, bid.BidId);


    try
    {
        var updatedAuction = await _auctionService.CreateBidToAuctionById(bid);

        return Ok(updatedAuction);
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
    }

    [HttpGet("{catalogId}/active")]
    public async Task<IActionResult> GetActiveAuctions(Guid catalogId)
    {
        _logger.LogInformation("GetActiveAuctions called for CatalogId: {CatalogId}", catalogId);

        var activeAuctions = await _auctionService.SendActiveAuctions(catalogId, AuctionStatus.Active);

        if (activeAuctions == null || !activeAuctions.Any())
        {
            _logger.LogWarning("GetActiveAuctions: No active auctions found for CatalogId {CatalogId}", catalogId);
            return NotFound();
        }

        return Ok(activeAuctions);
    }

    [HttpGet("{catalogId}/finished")]
    public async Task<IActionResult> GetFinishedAuctions(Guid catalogId)
    {
        _logger.LogInformation("GetFinishedAuctions called for CatalogId: {CatalogId}", catalogId);

        var finishedAuctions = await _auctionService.SendActiveAuctions(catalogId, AuctionStatus.Closed);

        if (finishedAuctions == null || !finishedAuctions.Any())
        {
            _logger.LogWarning("GetFinishedAuctions: No finished auctions found for CatalogId {CatalogId}", catalogId);
            return NotFound();
        }

        return Ok(finishedAuctions);
    }
}
