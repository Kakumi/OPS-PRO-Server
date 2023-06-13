using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class UserRoom : User
    {
        public DeckInfo? Deck { get; set; }
        public RPSChoice RPSChoice { get; set; }

        public bool Ready => Deck != null;

        public UserRoom(string connectionId, string username) : base(connectionId, username)
        {

        }

        public UserRoom(User user) : this(user.ConnectionId, user.Username)
        {

        }
    }
}
