using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
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
