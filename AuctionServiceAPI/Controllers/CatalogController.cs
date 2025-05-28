using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Diagnostics;
using AuctionServiceAPI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AuctionServiceAPI.Controllers
{
    [Route("api/catalog")]
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
                _logger.LogError(ex, "Failed to resolve IP address on startup");
            }
        }

        /// <summary>
        /// Henter alle aktive auktioner for i dag.
        /// </summary>
        [HttpGet("all-auctions-today")]
        public async Task<IActionResult> GetAllAuctionsToday()
        {
            _logger.LogInformation("GetAllAuctionsToday called");

            var auctions = await _catalogService.GetAllActiveAuctions();

            if (!auctions.Any())
            {
                _logger.LogWarning("No active auctions found");
                return NotFound("No active auctions today");
            }

            return Ok(auctions);
        }

        /// <summary>
        /// Importerer effekter fra StorageService til kataloget.
        /// </summary>
        [HttpPost("import-effects-from-storage")]
        public async Task<IActionResult> ImportEffectsFromStorage()
        {
            _logger.LogInformation("ImportEffectsFromStorage called");
            try
            {
                var auctions = await _catalogService.ImportEffectsFromStorageAsync();
                return Ok(auctions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed");
                return StatusCode(500, "Error importing effects from storage");
            }
        }

        /// <summary>
        /// Returnerer information om version og hosting IP.
        /// </summary>
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

            _logger.LogInformation("GetVersion called, returning service info");
            return props;
        }

        /// <summary>
        /// Henter alle kataloger.
        /// </summary>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCatalogs()
        {
            _logger.LogInformation("GetAllCatalogs called");

            var catalogs = await _catalogService.GetAllCatalogs();

            if (!catalogs.Any())
            {
                _logger.LogWarning("No catalogs found");
                return NotFound("No catalogs found");
            }

            return Ok(catalogs);
        }

        /// <summary>
        /// Opretter et nyt katalog.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCatalog([FromBody] Catalog catalog)
        {
            _logger.LogInformation("CreateCatalog called with Catalog: {@Catalog}", catalog);

            if (catalog == null)
            {
                _logger.LogWarning("CreateCatalog received null Catalog");
                return BadRequest("Catalog cannot be null");
            }

            try
            {
                var created = await _catalogService.CreateCatalog(catalog);
                _logger.LogInformation("Catalog created with ID: {CatalogId}", created.CatalogId);
                return CreatedAtAction(nameof(GetCatalogById), new { id = created.CatalogId }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating catalog");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create catalog");
            }
        }

        /// <summary>
        /// Henter et katalog ud fra dets ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatalogById(Guid id)
        {
            _logger.LogInformation("GetCatalogById called with ID: {Id}", id);

            var catalog = await _catalogService.GetCatalogById(id);
            if (catalog == null)
            {
                _logger.LogWarning("Catalog not found with ID: {Id}", id);
                return NotFound();
            }

            return Ok(catalog);
        }

        /// <summary>
        /// Sletter et katalog baseret på ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatalog(Guid id)
        {
            _logger.LogInformation("DeleteCatalog called with ID: {Id}", id);

            var result = await _catalogService.DeleteCatalog(id);
            if (!result)
            {
                _logger.LogWarning("DeleteCatalog failed. Catalog not found with ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("Catalog deleted with ID: {Id}", id);
            return NoContent();
        }

        /// <summary>
        /// Henter alle auktioner i et givent katalog. Kan filtreres efter status.
        /// </summary>
        [HttpGet("{catalogId}/auctions")]
        public async Task<IActionResult> GetAuctionsByCatalogId(Guid catalogId, [FromQuery] AuctionStatus? status)
        {
            _logger.LogInformation("GetAuctionsByCatalogId called for Catalog ID: {CatalogId} with Status filter: {Status}", catalogId, status);

            var auctions = await _catalogService.GetAuctionsByCatalogId(catalogId);

            if (status.HasValue)
                auctions = auctions.Where(a => a.Status == status).ToList();

            if (!auctions.Any())
            {
                _logger.LogWarning("No auctions found for Catalog ID: {CatalogId} with Status: {Status}", catalogId, status);
                return NotFound("No auctions found");
            }

            return Ok(auctions);
        }

        /// <summary>
        /// Håndterer afslutning af auktioner i et katalog.
        /// </summary>
        [HttpPost("{catalogId}/handle-finish")]
        public async Task<IActionResult> HandleAuctionFinish(Guid catalogId)
        {
            _logger.LogInformation("HandleAuctionFinish called for Catalog ID: {CatalogId}", catalogId);

            try
            {
                await _catalogService.HandleAuctionFinish(catalogId);
                _logger.LogInformation("Auction finish handled for Catalog ID: {CatalogId}", catalogId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling auction finish for Catalog ID: {CatalogId}", catalogId);
                return NotFound($"Catalog not found: {catalogId}");
            }
        }

        /// <summary>
        /// Afslutter et katalog og dets auktioner.
        /// </summary>
        [HttpPost("{catalogId}/end")]
        public async Task<IActionResult> EndCatalog(Guid catalogId)
        {
            _logger.LogInformation("EndCatalog called for Catalog ID: {CatalogId}", catalogId);

            try
            {
                await _catalogService.EndCatalog(catalogId);
                _logger.LogInformation("Catalog ended with ID: {CatalogId}", catalogId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ending catalog with ID: {CatalogId}", catalogId);
                return NotFound($"Catalog not found: {catalogId}");
            }
        }
    }
}
