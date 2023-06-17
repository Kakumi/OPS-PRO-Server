using Microsoft.VisualBasic;

namespace OPSProServer.Models
{
    public class DeckInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<CardInfo> Cards { get; set; }

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
            //if (Cards.Contains(cardInfo))
            //{
            //    Cards[cardInfo] -= amount;
            //    if (Cards[cardInfo] <= 0)
            //    {
            //        Cards.Remove(cardInfo);
            //    }
            //}
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
