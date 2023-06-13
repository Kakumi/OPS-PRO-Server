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
        public bool Ready => Deck != null;

        public UserRoom(Guid id, string connectionId, string username) : base(id, connectionId, username)
        {

        }

        public UserRoom(User user) : this(user.Id, user.ConnectionId, user.Username)
        {

        }
    }
}
