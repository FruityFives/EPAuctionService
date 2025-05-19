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
        }

        //endpoint for Semantic versioning
        [HttpGet("version")]
        public async Task<Dictionary<string, string>> GetVersion()
        {
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                properties.Add("hosted-at-address", "could not resolve IP-address");
            }

            return properties;
        }


        //Endpoint for create catalog
        [HttpPost("create")]
        public async Task<IActionResult> CreateCatalog([FromBody] Catalog catalog)
        {
            if (catalog == null)
            {
                return BadRequest("Catalog cannot be null");
            }

            // Call the service to create the catalog
            var createdCatalog = await _catalogService.CreateCatalog(catalog);

            if (createdCatalog == null)
            {
                return StatusCode(500, "An error occurred while creating the catalog");
            }

            return CreatedAtAction(nameof(GetCatalogById), new { id = createdCatalog.CatalogId }, createdCatalog);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatalogById(Guid id)
        {
            var catalog = await _catalogService.GetCatalogById(id);
            if (catalog == null)
            {
                return NotFound();
            }

            return Ok(catalog);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCatalog(Guid id)
        {
            var result = _catalogService.DeleteCatalog(id);
            if (result == null) return NotFound();
            return NoContent();
        }

        [HttpGet("{catalogId}/auctions")]
        public async Task<IActionResult> GetAuctionsByCatalogId(Guid catalogId)
        {
            var auctions = await _catalogService.GetAuctionsByCatalogId(catalogId);
            return Ok(auctions);
        }

        [HttpPost("{catalogId}/handle-finish")]
        public IActionResult HandleAuctionFinish(Guid catalogId)
        {
            var result = _catalogService.HandleAuctionFinish(catalogId);
            if (result == null) 
            { 
                return NotFound(); 
            }
            return NoContent();
        }

        //Get all catalogs
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCatalogs()
        {
            await Console.Out.WriteLineAsync("Hej");

            var catalogs = await _catalogService.GetAllCatalogs();
            if (catalogs == null || catalogs.Count == 0)
            {
                return NotFound("No catalogs found");
            }

            return Ok(catalogs);
        }


    }
}
