using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class FlowResponseMessage
    {
        public User? User { get; }
        public Room? Room { get; }
        public UserGameMessage UserGameMessage { get; }

        public FlowResponseMessage(User user, UserGameMessage userGameMessage)
        {
            User = user;
            Room = null;
            UserGameMessage = userGameMessage;
        }

        public FlowResponseMessage(Room room, UserGameMessage userGameMessage)
        {
            User = null;
            Room = room;
            UserGameMessage = userGameMessage;
        }

        public FlowResponseMessage(User user, string codeMessage, params string[] args) : this(user, new UserGameMessage(codeMessage, args)) { }

        public FlowResponseMessage(Room room, string codeMessage, params string[] args) : this(room, new UserGameMessage(codeMessage, args)) { }
    }
}
