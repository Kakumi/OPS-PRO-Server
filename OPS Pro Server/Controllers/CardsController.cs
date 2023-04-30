using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;

namespace OPS_Pro_Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ILogger<CardsController> _logger;
        private readonly IOptions<OpsPro> _config;

        public CardsController(ILogger<CardsController> logger, IOptions<OpsPro> config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                string allText = System.IO.File.ReadAllText(_config.Value.CardsPath!);
                var cards = JsonSerializer.Deserialize<List<Card>>(allText, options);

                return new JsonResult(cards);
            } catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Can't get cards data.");
            }
        }
    }
}