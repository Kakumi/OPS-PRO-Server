using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class UserAlertMessage
    {
        public string CodeMessage { get; }
        public string[] Args { get; }

        [JsonConstructor]
        public UserAlertMessage(string codeMessage, string[] args)
        {
            CodeMessage = codeMessage;
            Args = args;
        }
    }
}
