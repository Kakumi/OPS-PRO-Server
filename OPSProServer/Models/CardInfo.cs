namespace OPSProServer.Models
{
    public class CardInfo
    {
        public string Id { get; internal set; }
        public List<string>? Images { get; internal set; }
        public string Number { get; internal set; }
        public string Rarity { get; internal set; }
        public string CardType { get; internal set; }
        public string Name { get; internal set; }
        public int Cost { get; internal set; }
        public string? AttributeImage { get; internal set; }
        public string Attribute { get; internal set; }
        public int Power { get; internal set; }
        public int Counter { get; internal set; }
        public List<string> Colors { get; internal set; }
        public List<string> Types { get; internal set; }
        public List<string> Effects { get; internal set; }
        public string? Set { get; internal set; }

        internal CardInfo()
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
            get
            {
                if (CardType == "LEADER") return CardCategory.LEADER;
                if (CardType == "CHARACTER") return CardCategory.CHARACTER;
                if (CardType == "STAGE") return CardCategory.STAGE;
                if (CardType == "EVENT") return CardCategory.EVENT;

                return CardCategory.NONE;
            }
        }
    }
}
