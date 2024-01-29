using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class AttackableResult
    {
        public Guid Attacker { get; }
        public List<Guid> Cards { get; }

        public AttackableResult(Guid attacker, List<Guid> cards)
        {
            Attacker = attacker;
            Cards = cards;
        }
    }
}
