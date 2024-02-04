using OPSProServer.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace OPSProServer.Contracts.Models.Scripts
{
    public class CardScriptService
    {
        private static CardScriptService _instance;
        private readonly CardScript _default;
        private readonly Dictionary<string, CardScript> _scripts;

        private CardScriptService()
        {
            _default = new CardScript();
            _scripts = new Dictionary<string, CardScript>();
        }

        public static CardScriptService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CardScriptService();
                }
                return _instance;
            }
        }

        public CardScript GetScriptForCard(PlayingCard playingCard)
        {
            if (_scripts.ContainsKey(playingCard.CardInfo.GetScriptCode()))
            {
                return _scripts[playingCard.CardInfo.GetScriptCode()];
            }

            return _default;
        }

        public void AddCardScript(CardInfo cardInfo, CardScript script)
        {
            _scripts[cardInfo.GetScriptCode()] = script;
        }

        public int CountLoaded()
        {
            return _scripts.Keys.Count;
        }

        public CardScriptResult Load(List<CardInfo> cards)
        {
            _scripts.Clear();
            var allScriptsNumber = new List<string>();

            foreach (var card in cards)
            {
                var serieNumber = card.GetScriptCode();
                var script = Load(card);
                if (!allScriptsNumber.Contains(serieNumber))
                {
                    allScriptsNumber.Add(serieNumber);
                }
            }

            return new CardScriptResult(_scripts.Keys.Count, allScriptsNumber.Count);
        }

        private CardScript? Load(CardInfo cardInfo)
        {
            if (_scripts.ContainsKey(cardInfo.GetScriptCode()))
            {
                return _scripts[cardInfo.GetScriptCode()];
            }

            Assembly assembly = typeof(CardScript).Assembly;
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (typeof(CardScript).IsAssignableFrom(type))
                {
                    var attribute = type.GetCustomAttribute<CardScriptAttribute>();
                    if (attribute != null && attribute.GetScriptCode() == cardInfo.GetScriptCode())
                    {
                        ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
                        if (constructor != null)
                        {
                            CardScript? instance = Activator.CreateInstance(type) as CardScript;
                            if (instance != null)
                            {
                                _scripts[cardInfo.GetScriptCode()] = instance;

                                return instance;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
