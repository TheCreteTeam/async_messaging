using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace WebApplication1.Controllers;

// [Authorize]
[ApiController]
[Route("[controller]")]
// [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class WeatherForecastController : ControllerBase
{
    private readonly IDataProtector _dataProtector;
    
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, 
        IDataProtectionProvider dataProtectionProvider)
    {
        _logger = logger;
        _dataProtector = dataProtectionProvider.CreateProtector("WeatherForecastController");
    }

    [HttpGet("GetId")]
    public ValueTask<ActionResult<string>> GetRequestById(string? id)
    {
        if (id == null)
        {
            string test = "abcdef";
            return new ValueTask<ActionResult<string>>(_dataProtector.Protect(test));
        }
        else
        {
            return new ValueTask<ActionResult<string>>(_dataProtector.Unprotect(id));
        }
    }
        
    
    

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
}