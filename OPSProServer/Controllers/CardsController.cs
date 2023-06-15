using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OPSProServer.Models;
using System.Text.Json;

namespace OPSProServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    internal class CardsController : ControllerBase
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
                var cards = JsonSerializer.Deserialize<List<CardInfo>>(allText, options);

                return new JsonResult(cards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Can't get cards data.");
            }
        }
    }
}