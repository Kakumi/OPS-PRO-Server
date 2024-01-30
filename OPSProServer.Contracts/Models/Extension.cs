using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public static class Extension
    {
        public static List<Guid> Ids(this IEnumerable<PlayingCard> cards)
        {
            return cards.Select(x => x.Id).ToList();
        }
    }
}
