using Microsoft.Extensions.Options;
using OPSProServer.Contracts.Models;
using OPSProServer.Models;
using System.Text.Json;

namespace OPSProServer.Services
{
    public class CardService : ICardService
    {

        private readonly IOptions<OpsPro> _options;
        private List<CardInfo> _cards;

        public CardService(IOptions<OpsPro> options)
        {
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
            }

            return _cards;
        }

        public CardInfo? GetCardInfo(string id)
        {
            return _cards.FirstOrDefault(x => x.Id == id);
        }
    }
}
