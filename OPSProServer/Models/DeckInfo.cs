namespace OPSProServer.Models
{
    public class DeckInfo
    {
        public Guid Id { get; }
        public string Name { get; set; }
        public Dictionary<CardInfo, int> Cards { get; private set; }

        internal DeckInfo(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            Cards = new Dictionary<CardInfo, int>();
        }

        public void AddCard(CardInfo cardInfo, int amount = 1)
        {
            if (!Cards.ContainsKey(cardInfo))
            {
                Cards.Add(cardInfo, 0);
            }

            Cards[cardInfo] += amount;
        }

        public void RemoveCard(CardInfo cardInfo, int amount = 1)
        {
            if (Cards.ContainsKey(cardInfo))
            {
                Cards[cardInfo] -= amount;
                if (Cards[cardInfo] <= 0)
                {
                    Cards.Remove(cardInfo);
                }
            }
        }

        public int NumberOfCards => Cards.Sum(x => x.Value);

        public int NumberOfCardsTypes(params CardCategory[] types)
        {
            return Cards.Where(x => types.Contains(x.Key.CardCategory)).Sum(x => x.Value);
        }

        public DeckInfo Clone(string name)
        {
            var deck = new DeckInfo(name);
            deck.Cards = new Dictionary<CardInfo, int>(Cards.ToDictionary(entry => entry.Key, entry => entry.Value));

            return deck;
        }

        public bool IsValid()
        {
            var totalCards = NumberOfCardsTypes(CardCategory.CHARACTER, CardCategory.EVENT, CardCategory.STAGE);
            var totalLeader = NumberOfCardsTypes(CardCategory.LEADER);
            var leader = Cards.FirstOrDefault(x => x.Key.CardCategory == CardCategory.LEADER).Key;

            return totalCards == 50 && totalLeader == 1 && leader != null && Cards.All(x => x.Key.Colors.Any(y => leader.Colors.Contains(y)));
        }
    }
}
