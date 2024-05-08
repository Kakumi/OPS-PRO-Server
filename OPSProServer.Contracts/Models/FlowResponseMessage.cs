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
        public UserGameMessage UserGameMessage { get; }

        public FlowResponseMessage(User user, UserGameMessage userGameMessage)
        {
            User = user;
            UserGameMessage = userGameMessage;
        }

        public FlowResponseMessage(UserGameMessage userGameMessage)
        {
            User = null;
            UserGameMessage = userGameMessage;
        }

        public FlowResponseMessage(User user, string codeMessage, params string[] args) : this(user, new UserGameMessage(codeMessage, args)) { }

        public FlowResponseMessage(string codeMessage, params string[] args) : this(new UserGameMessage(codeMessage, args)) { }
    }
}
