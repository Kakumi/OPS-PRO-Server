using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class UserGameMessage
    {
        public string CodeMessage { get; }
        public string[] Args { get; }

        [JsonConstructor]
        public UserGameMessage(string codeMessage, params string[] args)
        {
            CodeMessage = codeMessage;
            Args = args;
        }
    }
}
