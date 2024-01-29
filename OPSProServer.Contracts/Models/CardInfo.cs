using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class CardInfo
    {
        public string Id { get; private set; }
        public List<string>? Images { get; private set; }
        public string Number { get; private set; }
        public string Rarity { get; private set; }
        public string CardType { get; private set; }
        public string Name { get; private set; }
        public int Cost { get; private set; }
        public string? AttributeImage { get; private set; }
        public string Attribute { get; private set; }
        public int Power { get; private set; }
        public int Counter { get; private set; }
        public List<string> Colors { get; private set; }
        public List<string> Types { get; private set; }
        public List<string> Effects { get; private set; }
        public string? Set { get; private set; }
        public bool IsBlocker { get; private set; }
        public bool IsRush { get; private set; }
        public bool IsDoubleAttack { get; private set; }
        public bool IsBanish { get; private set; }
        public bool IsTrigger { get; private set; }

        [JsonConstructor]
        public CardInfo(string id, List<string>? images, string number, string rarity, string cardType, string name, int cost, string? attributeImage, string attribute, int power, int counter, List<string> colors, List<string> types, List<string> effects, string? set, bool isBlocker, bool isRush, bool isDoubleAttack, bool isBanish, bool isTrigger)
        {
            Id = id;
            Images = images;
            Number = number;
            Rarity = rarity;
            CardType = cardType;
            Name = name;
            Cost = cost;
            AttributeImage = attributeImage;
            Attribute = attribute;
            Power = power;
            Counter = counter;
            Colors = colors;
            Types = types;
            Effects = effects;
            Set = set;
            IsBlocker = isBlocker;
            IsRush = isRush;
            IsDoubleAttack = isDoubleAttack;
            IsBanish = isBanish;
            IsTrigger = isTrigger;
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
