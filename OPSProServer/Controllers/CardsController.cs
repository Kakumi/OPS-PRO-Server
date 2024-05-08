using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OPSProServer.Contracts.Models;
using OPSProServer.Models;
using OPSProServer.Services;
using System.Text.Json;

namespace OPSProServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ILogger<CardsController> _logger;
        private readonly ICardService _cardService;

        public CardsController(ILogger<CardsController> logger, ICardService cardService)
        {
            _logger = logger;
            _cardService = cardService;
        }

        [HttpGet]
        [Produces("application/json")]
        public IActionResult Get()
        {
            try
            {
                return new JsonResult(_cardService.GetCardsInfo());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest("Can't get cards data.");
            }
        }
    }
}