namespace OPSProServer.Models
{
    public class PlayingCard
    {
        public Guid Id { get; set; }
        public CardInfo CardInfo { get; set; }
        public List<KeyValuePair<ModifierDuration, int>> CostModifier { get; set; }
        public List<KeyValuePair<ModifierDuration, int>> CounterModifier { get; set; }
        public List<KeyValuePair<ModifierDuration, int>> PowerModifier { get; set; }
        public bool Rested { get; set; }
        public bool Flipped { get; set; }
        public bool Destructable { get; set; }
        public bool VisibleForOpponent { get; set; }

        public PlayingCard(CardInfo cardInfo)
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

        public void ToggleRested()
        {
            Rested = !Rested;
        }

        public void ToggleFlipped()
        {
            Flipped = !Flipped;
        }

        public void ToggleDestructable()
        {
            Destructable = !Destructable;
        }

        public void ToggleVisibleForOpponent()
        {
            VisibleForOpponent = !VisibleForOpponent;
        }

        public int GetCustomPower()
        {
            return PowerModifier.Sum(x => x.Value);
        }

        public int GetTotalPower()
        {
            return CardInfo.Power + GetCustomPower();
        }

        public int GetCustomCost()
        {
            return CostModifier.Sum(x => x.Value);
        }

        public int GetTotalCost()
        {
            return CardInfo.Cost + GetCustomCost();
        }

        public int GetCustomCounter()
        {
            return CounterModifier.Sum(x => x.Value);
        }

        public int GetTotalCounter()
        {
            return CardInfo.Counter + GetCustomCounter();
        }

        public void RemoveStatDuration(ModifierDuration type)
        {
            PowerModifier = PowerModifier.Where(x => x.Key != type).ToList();
            CostModifier = CostModifier.Where(x => x.Key != type).ToList();
            CounterModifier = CounterModifier.Where(x => x.Key != type).ToList();
        }
    }
}
