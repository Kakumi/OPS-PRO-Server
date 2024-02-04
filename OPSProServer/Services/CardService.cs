using Microsoft.Extensions.Options;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Models.Scripts;
using OPSProServer.Models;
using System.Text.Json;

namespace OPSProServer.Services
{
    public class CardService : ICardService
    {
        private readonly ILogger<CardService> _logger;
        private readonly IOptions<OpsPro> _options;
        private List<CardInfo> _cards;

        public CardService(ILogger<CardService> logger, IOptions<OpsPro> options)
        {
            _logger = logger;
            _options = options;
            _cards = new List<CardInfo>();
        }

        public IEnumerable<CardInfo> GetCardsInfo()
        {
            if (_cards.Count == 0)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                string allText = File.ReadAllText(_options.Value.CardsPath!);
                _cards = JsonSerializer.Deserialize<List<CardInfo>>(allText, options)!;
                var scriptResult = CardScriptService.Instance.Load(_cards);

                _logger.LogInformation("{Count} card(s) found.", _cards.Count);
                _logger.LogInformation("{Count}/{NbScripts} script(s) found.", scriptResult.Loaded, scriptResult.Total);
            }

            return _cards;
        }

        public CardInfo? GetCardInfo(string id)
        {
            return GetCardsInfo().FirstOrDefault(x => x.Id == id);
        }
    }
}
