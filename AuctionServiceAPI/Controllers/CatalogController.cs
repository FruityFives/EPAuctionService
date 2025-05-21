using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Diagnostics;
using AuctionServiceAPI.Services;

namespace AuctionServiceAPI.Controllers
{
    [Route("api/[controller]")]
[ApiController]
public class CatalogController : ControllerBase
{
    private readonly ILogger<CatalogController> _logger;
    private readonly ICatalogService _catalogService;

    public CatalogController(ILogger<CatalogController> logger, ICatalogService catalogService)
    {
        _logger = logger;
        _catalogService = catalogService;

        try
        {
            var ip = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName())
                                   .First().MapToIPv4().ToString();
            _logger.LogInformation($"CatalogServiceAPI responding from {ip}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to resolve IP: {ex.Message}");
        }
    }

    [HttpGet("version")]
    public async Task<Dictionary<string, string>> GetVersion()
    {
        var props = new Dictionary<string, string>
        {
            ["service"] = "AuctionServiceAPI",
            ["version"] = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion
        };

        try
        {
            var ip = (await System.Net.Dns.GetHostAddressesAsync(System.Net.Dns.GetHostName()))
                    .First().MapToIPv4().ToString();
            props.Add("hosted-at-address", ip);
        }
        catch
        {
            props.Add("hosted-at-address", "unresolved");
        }

        return props;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCatalog([FromBody] Catalog catalog)
    {
        if (catalog == null) return BadRequest("Catalog cannot be null");

        var created = await _catalogService.CreateCatalog(catalog);
        return CreatedAtAction(nameof(GetCatalogById), new { id = created.CatalogId }, created);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCatalogById(Guid id)
    {
        var catalog = await _catalogService.GetCatalogById(id);
        return catalog == null ? NotFound() : Ok(catalog);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCatalog(Guid id)
    {
        var result = await _catalogService.DeleteCatalog(id);
        return result ? NoContent() : NotFound();
    }

    [HttpGet("{catalogId}/auctions")]
public async Task<IActionResult> GetAuctionsByCatalogId(Guid catalogId, [FromQuery] AuctionStatus? status)
{
    var auctions = await _catalogService.GetAuctionsByCatalogId(catalogId);

    if (status.HasValue)
        auctions = auctions.Where(a => a.Status == status).ToList();

    return auctions.Any() ? Ok(auctions) : NotFound("No auctions found");
}

    [HttpPost("{catalogId}/handle-finish")]
    public async Task<IActionResult> HandleAuctionFinish(Guid catalogId)
    {
        try
        {
            await _catalogService.HandleAuctionFinish(catalogId);
            return NoContent();
        }
        catch
        {
            return NotFound($"Catalog not found: {catalogId}");
        }
    }

    [HttpPost("{catalogId}/end")]
    public async Task<IActionResult> EndCatalog(Guid catalogId)
    {
        await _catalogService.EndCatalog(catalogId);
        return NoContent();
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllCatalogs()
    {
        var catalogs = await _catalogService.GetAllCatalogs();
        return catalogs.Any() ? Ok(catalogs) : NotFound("No catalogs found");
    }
}


}
