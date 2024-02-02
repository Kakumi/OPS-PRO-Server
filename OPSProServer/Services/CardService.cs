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
        private Dictionary<string, ICardScript> _scripts;

        public CardService(ILogger<CardService> logger, IOptions<OpsPro> options)
        {
            _logger = logger;
            _options = options;
            _cards = new List<CardInfo>();
            _scripts = new Dictionary<string, ICardScript>();
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

                var allScriptsNumber = new List<string>();
                foreach (var card in _cards)
                {
                    var serieNumber = card.GetScriptCode();
                    var script = GetCardScript(serieNumber);
                    if (!allScriptsNumber.Contains(serieNumber))
                    {
                        allScriptsNumber.Add(serieNumber);
                    }

                    if (script != null && !_scripts.ContainsKey(serieNumber))
                    {
                        _scripts.Add(serieNumber, script);
                    }
                }

                var nbScripts = allScriptsNumber.Count;

                _logger.LogInformation("{Count} card(s) found.", _cards.Count);
                _logger.LogInformation("{Count}/{NbScripts} script(s) found.", _scripts.Values.Count, nbScripts);
            }

            return _cards;
        }

        public CardInfo? GetCardInfo(string id)
        {
            return GetCardsInfo().FirstOrDefault(x => x.Id == id);
        }

        public ICardScript? GetCardScript(string serieNumber)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (typeof(ICardScript).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<CardScriptAttribute>();
                    if (attribute != null && attribute.GetScriptCode() == serieNumber)
                    {
                        ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
                        if (constructor != null)
                        {
                            ICardScript? instance = Activator.CreateInstance(type) as ICardScript;
                            if (instance != null)
                            {
                                return instance;
                            }
                        }
                    }
                }
            }

            _logger.LogWarning("Script file for number {Id} doesn't exists.", serieNumber);

            return null;
        }

        public ICardScript? GetCardScript(PlayingCard playingCard)
        {
            if (_scripts.ContainsKey(playingCard.CardInfo.GetScriptCode()))
            {
                return _scripts[playingCard.CardInfo.GetScriptCode()];
            }

            return null;
        }

        public bool IsBlocker(PlayingCard playingCard, User user, Game game)
        {
            if (playingCard.CardInfo.IsBlocker)
            {
                return true;
            }

            var script = GetCardScript(playingCard);
            if (script != null)
            {
                return script.IsBlocker(user, game, playingCard);
            }

            return false;
        }

        public bool IsRusher(PlayingCard playingCard, User user, Game game)
        {
            if (playingCard.CardInfo.IsRush)
            {
                return true;
            }

            var script = GetCardScript(playingCard);
            if (script != null)
            {
                return script.IsRusher(user, game, playingCard);
            }

            return false;
        }

        public bool IsDoubleAttack(PlayingCard playingCard, User user, Game game)
        {
            if (playingCard.CardInfo.IsDoubleAttack)
            {
                return true;
            }

            var script = GetCardScript(playingCard);
            if (script != null)
            {
                return script.IsDoubleAttack(user, game, playingCard);
            }

            return false;
        }

        public bool IsBanish(PlayingCard playingCard, User user, Game game)
        {
            if (playingCard.CardInfo.IsBanish)
            {
                return true;
            }

            var script = GetCardScript(playingCard);
            if (script != null)
            {
                return script.IsBanish(user, game, playingCard);
            }

            return false;
        }
    }
}
