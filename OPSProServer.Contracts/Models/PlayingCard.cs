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
        public List<ValueModifier> CostModifier { get; private set; }
        public List<ValueModifier> CounterModifier { get; private set; }
        public List<ValueModifier> PowerModifier { get; private set; }
        public List<TagModifier> Tags { get; private set; }
        public bool Rested { get; set; }
        public bool Flipped { get; set; }
        public bool Destructable { get; set; }
        public bool VisibleForOpponent { get; set; }
        public int Turn { get; private set; }
        public int DonCard { get; set; }

        [JsonConstructor]
        public PlayingCard(Guid id, CardInfo cardInfo, List<ValueModifier> costModifier, List<ValueModifier> counterModifier, List<ValueModifier> powerModifier, List<TagModifier> tags, bool rested, bool flipped, bool destructable, bool visibleForOpponent, int turn, int donCard)
        {
            Id = id;
            CardInfo = cardInfo;
            CostModifier = costModifier;
            CounterModifier = counterModifier;
            PowerModifier = powerModifier;
            Tags = tags;
            Rested = rested;
            Flipped = flipped;
            Destructable = destructable;
            VisibleForOpponent = visibleForOpponent;
            Turn = turn;
            DonCard = donCard;
        }

        public PlayingCard(CardInfo cardInfo)
        {
            Id = Guid.NewGuid();
            CardInfo = cardInfo;
            CostModifier = new List<ValueModifier>();
            CounterModifier = new List<ValueModifier>();
            PowerModifier = new List<ValueModifier>();
            Tags = new List<TagModifier>();
            Rested = false;
            Flipped = false;
            Destructable = false;
            VisibleForOpponent = false;
            Turn = 1;
            DonCard = 0;
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
            return PowerModifier.Sum(x => x.Value) + (DonCard * 1000);
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
            PowerModifier.RemoveAll(x => x.Duration == type);
            CostModifier.RemoveAll(x => x.Duration == type);
            CounterModifier.RemoveAll(x => x.Duration == type);
            Tags.RemoveAll(x => x.Duration == type);
        }

        public void IncrementTurn()
        {
            Turn++;
        }

        internal void ResetTurn()
        {
            Turn = 1;
        }

        public bool HasTag(string tag)
        {
            return Tags.Any(x => x.Value == tag);
        }

        public void SetOncePerTurnTag()
        {
            Tags.Add(new TagModifier(ModifierDuration.OpponentTurn, "once_per_turn"));
        }

        public bool HasOncePerTurn()
        {
            return Tags.Any(x => x.Value == "once_per_turn");
        }
    }
}
