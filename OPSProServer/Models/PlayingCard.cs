namespace OPSProServer.Models
{
    public class PlayingCard
    {
        public Guid Id { get; }
        public CardInfo CardInfo { get; }
        public List<KeyValuePair<ModifierDuration, int>> CostModifier { get; private set; }
        public List<KeyValuePair<ModifierDuration, int>> CounterModifier { get; private set; }
        public List<KeyValuePair<ModifierDuration, int>> PowerModifier { get; private set; }
        public bool Rested { get; internal set; }
        public bool Flipped { get; internal set; }
        public bool Destructable { get; internal set; }
        public bool VisibleForOpponent { get; internal set; }

        internal PlayingCard(CardInfo cardInfo)
        {
            Id = Guid.NewGuid();
            CardInfo = cardInfo;
            CostModifier = new List<KeyValuePair<ModifierDuration, int>>();
            CounterModifier = new List<KeyValuePair<ModifierDuration, int>>();
            PowerModifier = new List<KeyValuePair<ModifierDuration, int>>();
            Rested = false;
            Flipped = false;
            Destructable = false;
            VisibleForOpponent = false;
        }
    }
}
