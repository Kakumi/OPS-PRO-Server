namespace OPSProServer.Contracts.Contracts
{
    public class CardInfo
    {
        public string Id { get; set; }
        public List<string>? Images { get; set; }
        public string Number { get; set; }
        public string Rarity { get; set; }
        public string CardType { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public string? AttributeImage { get; set; }
        public string Attribute { get; set; }
        public int Power { get; set; }
        public int Counter { get; set; }
        public List<string> Colors { get; set; }
        public List<string> Types { get; set; }
        public List<string> Effects { get; set; }
        public string? Set { get; set; }

        public CardInfo()
        {
            Id = string.Empty;
            Number = string.Empty;
            Rarity = string.Empty;
            CardType = string.Empty;
            Name = string.Empty;
            Attribute = string.Empty;
            Cost = 0;
            Power = 0;
            Counter = 0;

            Colors = new List<string>();
            Types = new List<string>();
            Effects = new List<string>();
        }

        public CardCategory CardCategory
        {
            get {
                if (CardType == "LEADER") return CardCategory.LEADER;
                if (CardType == "CHARACTER") return CardCategory.CHARACTER;
                if (CardType == "STAGE") return CardCategory.STAGE;
                if (CardType == "EVENT") return CardCategory.EVENT;

                return CardCategory.NONE;
            }
        }
    }
}
