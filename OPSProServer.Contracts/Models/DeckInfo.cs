using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class DeckInfo
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public List<CardInfo> Cards { get; private set; } //Dictionary with complex model does not work with SignalR

        [JsonConstructor]
        public DeckInfo(Guid id, string name, List<CardInfo> cards)
        {
            Id = id;
            Name = name;
            Cards = cards;
        }

        public DeckInfo(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            Cards = new List<CardInfo>();
        }

        public void AddCard(CardInfo cardInfo, int amount = 1)
        {
            Cards.AddRange(Enumerable.Repeat(cardInfo, amount));
        }

        public void RemoveCard(CardInfo cardInfo, int amount = 1)
        {
            int count = 0;
            for (int i = 0; i < Cards.Count; i++)
            {
                if (Cards[i].Id == cardInfo.Id)
                {
                    count++;
                    if (count <= amount)
                    {
                        Cards.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        public int NumberOfCards => Cards.Count;

        public int NumberOfCardsTypes(params CardCategory[] types)
        {
            return Cards.Where(x => types.Contains(x.CardCategory)).Count();
        }

        public CardInfo? GetLeader()
        {
            return Cards.FirstOrDefault(x => x.CardCategory == CardCategory.LEADER);
        }

        public List<CardInfo> GetCards()
        {
            return Cards.Where(x => x.CardCategory == CardCategory.CHARACTER || x.CardCategory == CardCategory.STAGE || x.CardCategory == CardCategory.EVENT).ToList();
        }

        public DeckInfo Clone(string name)
        {
            var deck = new DeckInfo(name);
            deck.Cards = new List<CardInfo>(Cards.ToList());

            return deck;
        }

        public bool IsValid()
        {
            var totalCards = NumberOfCardsTypes(CardCategory.CHARACTER, CardCategory.EVENT, CardCategory.STAGE);
            var totalLeader = NumberOfCardsTypes(CardCategory.LEADER);
            var leader = Cards.FirstOrDefault(x => x.CardCategory == CardCategory.LEADER);

            return totalCards == 50 && totalLeader == 1 && leader != null && Cards.All(x => x.Colors.Any(y => leader.Colors.Contains(y)));
        }
    }
}
