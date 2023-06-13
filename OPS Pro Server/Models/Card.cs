namespace OPSProServer.Contracts.Contracts
{
    public class Card : CardInfo
    {
        public string? Id { get; set; }
        public List<string>? Images { get; set; }
        public string? Number { get; set; }
        public string? Rarity { get; set; }
        public string? CardType { get; set; }
        public string? Name { get; set; }
        public int? Cost { get; set; }
        public string? AttributeImage { get; set; }
        public string? Attribute { get; set; }
        public int? Power { get; set; }
        public int? Counter { get; set; }
        public List<string>? Colors { get; set; }
        public List<string>? Types { get; set; }
        public List<string>? Effects { get; set; }
        public string? Set { get; set; }
    }
}
