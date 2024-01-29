using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class PlayingCard
    {
        public Guid Id { get; private set; }
        public CardInfo CardInfo { get; private set; }
        public List<KeyValuePair<ModifierDuration, int>> CostModifier { get; private set; }
        public List<KeyValuePair<ModifierDuration, int>> CounterModifier { get; private set; }
        public List<KeyValuePair<ModifierDuration, int>> PowerModifier { get; private set; }
        public bool Rested { get; set; }
        public bool Flipped { get; set; }
        public bool Destructable { get; set; }
        public bool VisibleForOpponent { get; set; }
        public int Turn { get; private set; }

        [JsonConstructor]
        public PlayingCard(Guid id, CardInfo cardInfo, List<KeyValuePair<ModifierDuration, int>> costModifier, List<KeyValuePair<ModifierDuration, int>> counterModifier, List<KeyValuePair<ModifierDuration, int>> powerModifier, bool rested, bool flipped, bool destructable, bool visibleForOpponent, int turn)
        {
            Id = id;
            CardInfo = cardInfo;
            CostModifier = costModifier;
            CounterModifier = counterModifier;
            PowerModifier = powerModifier;
            Rested = rested;
            Flipped = flipped;
            Destructable = destructable;
            VisibleForOpponent = visibleForOpponent;
            Turn = turn;
        }

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
            Turn = 1;
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

        public void IncrementTurn()
        {
            Turn++;
        }

        internal void ResetTurn()
        {
            Turn = 0;
        }
    }
}
