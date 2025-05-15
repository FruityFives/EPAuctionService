using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AuctionServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionController : ControllerBase
{
   
    private readonly ILogger<AuctionController> _logger;
// herhenne
    public AuctionController(ILogger<AuctionController> logger)
    {
        _logger = logger;
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

        try {
            var hostName = System.Net.Dns.GetHostName();
            var ips = await System.Net.Dns.GetHostAddressesAsync(hostName);
            var ipa = ips.First().MapToIPv4().ToString();
            properties.Add("hosted-at-address", ipa);
        } catch (Exception ex) {
            _logger.LogError(ex.Message);
            properties.Add("hosted-at-address", "could not resolve IP-address");
        }

        return properties;
    }


    
}
