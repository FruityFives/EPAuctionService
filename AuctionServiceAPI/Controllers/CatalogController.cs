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
    /// <summary>
/// Controller til håndtering af kataloger og tilknyttede auktioner.
/// </summary>
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
            _logger.LogInformation($"CatalogServiceAPI svarer fra {ip}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kunne ikke hente IP-adresse ved opstart");
        }
    }

    /// <summary>
    /// Henter version og host-information for CatalogServiceAPI.
    /// </summary>
    /// <returns>Dictionary med service-navn, version og IP-adresse.</returns>
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

        _logger.LogInformation("GetVersion kaldt, returnerer service-info");
        return props;
    }

    /// <summary>
    /// Opretter et nyt katalog.
    /// </summary>
    /// <param name="catalog">Katalogobjektet der skal oprettes.</param>
    /// <returns>Returnerer det oprettede katalog og dets placering.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCatalog([FromBody] Catalog catalog)
    {
        _logger.LogInformation("CreateCatalog kaldt med katalog: {@Catalog}", catalog);

        if (catalog == null)
        {
            _logger.LogWarning("CreateCatalog modtog null-katalog");
            return BadRequest("Catalog må ikke være null");
        }

        try
        {
            var created = await _catalogService.CreateCatalog(catalog);
            _logger.LogInformation("Katalog oprettet med ID: {CatalogId}", created.CatalogId);
            return CreatedAtAction(nameof(GetCatalogById), new { id = created.CatalogId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved oprettelse af katalog");
            return StatusCode(StatusCodes.Status500InternalServerError, "Fejl ved oprettelse af katalog");
        }
    }

    /// <summary>
    /// Henter et katalog baseret på dets ID.
    /// </summary>
    /// <param name="id">Katalogets ID.</param>
    /// <returns>Returnerer kataloget hvis det findes, ellers 404.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCatalogById(Guid id)
    {
        _logger.LogInformation("GetCatalogById kaldt med ID: {Id}", id);

        var catalog = await _catalogService.GetCatalogById(id);
        if (catalog == null)
        {
            _logger.LogWarning("Intet katalog fundet med ID: {Id}", id);
            return NotFound();
        }

        return Ok(catalog);
    }

    /// <summary>
    /// Sletter et katalog ud fra dets ID.
    /// </summary>
    /// <param name="id">ID på det katalog der skal slettes.</param>
    /// <returns>NoContent hvis slettet, ellers NotFound.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCatalog(Guid id)
    {
        _logger.LogInformation("DeleteCatalog kaldt med ID: {Id}", id);

        var result = await _catalogService.DeleteCatalog(id);
        if (!result)
        {
            _logger.LogWarning("Sletning fejlede. Intet katalog fundet med ID: {Id}", id);
            return NotFound();
        }

        _logger.LogInformation("Katalog slettet med ID: {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Henter alle auktioner tilknyttet et katalog ID, med valgfri filtrering på status.
    /// </summary>
    /// <param name="catalogId">ID på det katalog, hvorfra auktioner skal hentes.</param>
    /// <param name="status">Valgfri filtrering på auktionsstatus.</param>
    /// <returns>En liste af auktioner eller NotFound hvis tom.</returns>
    [HttpGet("{catalogId}/auctions")]
    public async Task<IActionResult> GetAuctionsByCatalogId(Guid catalogId, [FromQuery] AuctionStatus? status)
    {
        _logger.LogInformation("GetAuctionsByCatalogId kaldt for katalog ID: {CatalogId} med status-filter: {Status}", catalogId, status);

        var auctions = await _catalogService.GetAuctionsByCatalogId(catalogId);

        if (status.HasValue)
            auctions = auctions.Where(a => a.Status == status).ToList();

        if (!auctions.Any())
        {
            _logger.LogWarning("Ingen auktioner fundet for katalog ID: {CatalogId} med status: {Status}", catalogId, status);
            return NotFound("Ingen auktioner fundet");
        }

        return Ok(auctions);
    }

    /// <summary>
    /// Håndterer afslutningen af auktioner i et katalog.
    /// </summary>
    /// <param name="catalogId">ID på kataloget hvor auktioner skal afsluttes.</param>
    /// <returns>NoContent hvis succesfuldt, ellers NotFound ved fejl.</returns>
    [HttpPost("{catalogId}/handle-finish")]
    public async Task<IActionResult> HandleAuctionFinish(Guid catalogId)
    {
        _logger.LogInformation("HandleAuctionFinish kaldt for katalog ID: {CatalogId}", catalogId);

        try
        {
            await _catalogService.HandleAuctionFinish(catalogId);
            _logger.LogInformation("Auktioner afsluttet for katalog ID: {CatalogId}", catalogId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved afslutning af auktioner for katalog ID: {CatalogId}", catalogId);
            return NotFound($"Katalog ikke fundet: {catalogId}");
        }
    }

    /// <summary>
    /// Afslutter hele kataloget og dets relaterede auktioner.
    /// </summary>
    /// <param name="catalogId">ID på kataloget der skal afsluttes.</param>
    /// <returns>NoContent hvis succesfuldt, ellers NotFound.</returns>
    [HttpPost("{catalogId}/end")]
    public async Task<IActionResult> EndCatalog(Guid catalogId)
    {
        _logger.LogInformation("EndCatalog kaldt for katalog ID: {CatalogId}", catalogId);

        try
        {
            await _catalogService.EndCatalog(catalogId);
            _logger.LogInformation("Katalog afsluttet med ID: {CatalogId}", catalogId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fejl ved afslutning af katalog med ID: {CatalogId}", catalogId);
            return NotFound($"Katalog ikke fundet: {catalogId}");
        }
    }

    /// <summary>
    /// Henter alle kataloger.
    /// </summary>
    /// <returns>En liste af alle kataloger eller NotFound hvis ingen findes.</returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllCatalogs()
    {
        _logger.LogInformation("GetAllCatalogs kaldt");

        var catalogs = await _catalogService.GetAllCatalogs();

        if (!catalogs.Any())
        {
            _logger.LogWarning("Ingen kataloger fundet");
            return NotFound("Ingen kataloger fundet");
        }

        return Ok(catalogs);
    }
}


}
