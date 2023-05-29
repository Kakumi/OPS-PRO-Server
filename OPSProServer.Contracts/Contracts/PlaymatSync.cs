using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class PlaymatSync
    {
        public Guid UserId { get; set; }
        public Guid Leader { get; set; }
        public Guid Life { get; set; }
        public Guid Deck { get; set; }
        public Guid Stage { get; set; }
        public Guid Trash { get; set; }
        public Guid Cost { get; set; }
        public Guid DonDeck { get; set; }
        public List<Guid> Characters { get; set; } = new List<Guid>();
    }
}
