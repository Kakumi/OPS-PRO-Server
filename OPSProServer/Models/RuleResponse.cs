using OPSProServer.Contracts.Models;

namespace OPSProServer.Models
{
    public class RuleResponse
    {
        public List<UserGameMessage> CodesMessage { get; set; }
        public List<UserResolver> Resolvers { get; set; }

        public RuleResponse()
        {
            CodesMessage = new List<UserGameMessage>();
            Resolvers = new List<UserResolver>();
        }
    }
}
