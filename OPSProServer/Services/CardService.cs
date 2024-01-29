using Microsoft.Extensions.Options;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Models;
using OPSProServer.Models;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            }

            return _cards;
        }

        public CardInfo? GetCardInfo(string id)
        {
            return _cards.FirstOrDefault(x => x.Id == id);
        }

        public ICardScript? GetCardScript(PlayingCard playingCard)
        {
            var args = playingCard.CardInfo.Number.Split("-");
            if (args.Length == 2)
            {
                var serie = args[0];
                var number = args[1];

                Assembly assembly = Assembly.GetExecutingAssembly();
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (typeof(ICardScript).IsAssignableFrom(type))
                    {
                        var attribute = type.GetCustomAttribute<CardScriptAttribute>();
                        if (attribute != null && attribute.Serie == serie && attribute.Number == number)
                        {
                            ConstructorInfo? constructor = type.GetConstructor(new[] { typeof(PlayingCard) });
                            if (constructor != null)
                            {
                                ICardScript instance = Activator.CreateInstance(type, playingCard) as ICardScript;
                                return instance;
                            }
                        }
                    }
                }

                _logger.LogWarning("Script file for serie {Serie} with number {Number} doesn't exists.", serie, number);
            }

            _logger.LogError("Invalid card number {Number}", playingCard.CardInfo.Number);

            return null;
        }
    }
}
