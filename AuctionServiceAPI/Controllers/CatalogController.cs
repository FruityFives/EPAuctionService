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
                var hostName = System.Net.Dns.GetHostName();
                var ips = System.Net.Dns.GetHostAddresses(hostName);
                var ipAddr = ips.First().MapToIPv4().ToString();
                _logger.LogInformation($"CatalogServiceAPI responding from {ipAddr}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to resolve IP address: {ex.Message}");
            }
        }

        // Endpoint for Semantic versioning
        [HttpGet("version")]
        public async Task<Dictionary<string, string>> GetVersion()
        {
            _logger.LogInformation("Fetching service version info");

            var properties = new Dictionary<string, string>();
            var assembly = typeof(Program).Assembly;

            properties.Add("service", "AuctionServiceAPI");
            var ver = FileVersionInfo.GetVersionInfo(typeof(Program).Assembly.Location).ProductVersion;
            properties.Add("version", ver);

            try
            {
                var hostName = System.Net.Dns.GetHostName();
                var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
                var ipa = ips.First().MapToIPv4().ToString();
                properties.Add("hosted-at-address", ipa);
                _logger.LogInformation($"Version info retrieved, hosted at {ipa}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching IP address for version info: {ex.Message}");
                properties.Add("hosted-at-address", "could not resolve IP-address");
            }

            return properties;
        }

        // Endpoint for creating catalog
        [HttpPost("create")]
        public async Task<IActionResult> CreateCatalog([FromBody] Catalog catalog)
        {
            if (catalog == null)
            {
                _logger.LogWarning("CreateCatalog called with null catalog");
                return BadRequest("Catalog cannot be null");
            }

            _logger.LogInformation("Creating new catalog with name: {CatalogName}", catalog.Name);

            var createdCatalog = await _catalogService.CreateCatalog(catalog);

            if (createdCatalog == null)
            {
                _logger.LogError("Failed to create catalog with name: {CatalogName}", catalog.Name);
                return StatusCode(500, "An error occurred while creating the catalog");
            }

            _logger.LogInformation("Catalog created with ID: {CatalogId}", createdCatalog.CatalogId);
            return CreatedAtAction(nameof(GetCatalogById), new { id = createdCatalog.CatalogId }, createdCatalog);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatalogById(Guid id)
        {
            _logger.LogInformation("Fetching catalog by ID: {CatalogId}", id);
            var catalog = await _catalogService.GetCatalogById(id);

            if (catalog == null)
            {
                _logger.LogWarning("Catalog not found with ID: {CatalogId}", id);
                return NotFound();
            }

            _logger.LogInformation("Catalog found with ID: {CatalogId}", id);
            return Ok(catalog);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCatalog(Guid id)
        {
            _logger.LogInformation("Deleting catalog with ID: {CatalogId}", id);
            var result = _catalogService.DeleteCatalog(id);

            if (result == null)
            {
                _logger.LogWarning("Catalog to delete not found with ID: {CatalogId}", id);
                return NotFound();
            }

            _logger.LogInformation("Catalog deleted with ID: {CatalogId}", id);
            return NoContent();
        }

        [HttpGet("{catalogId}/auctions")]
        public async Task<IActionResult> GetAuctionsByCatalogId(Guid catalogId)
        {
            _logger.LogInformation("Fetching auctions for catalog ID: {CatalogId}", catalogId);
            var auctions = await _catalogService.GetAuctionsByCatalogId(catalogId);
            return Ok(auctions);
        }

        [HttpPost("{catalogId}/handle-finish")]
        public IActionResult HandleAuctionFinish(Guid catalogId)
        {
            _logger.LogInformation("Handling auction finish for catalog ID: {CatalogId}", catalogId);
            var result = _catalogService.HandleAuctionFinish(catalogId);

            if (result == null)
            {
                _logger.LogWarning("No catalog found to handle auction finish with ID: {CatalogId}", catalogId);
                return NotFound();
            }

            _logger.LogInformation("Handled auction finish for catalog ID: {CatalogId}", catalogId);
            return NoContent();
        }

        // Get all catalogs
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCatalogs()
        {
            _logger.LogInformation("Fetching all catalogs");
            var catalogs = await _catalogService.GetAllCatalogs();

            if (catalogs == null || catalogs.Count == 0)
            {
                _logger.LogWarning("No catalogs found");
                return NotFound("No catalogs found");
            }

            _logger.LogInformation("{Count} catalogs found", catalogs.Count);
            return Ok(catalogs);
        }
    }
}
