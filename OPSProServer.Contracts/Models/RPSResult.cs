using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class RPSResult
    {
        public Dictionary<Guid, RPSChoice> Signs { get; private set; }
        public Guid? Winner { get; }

        public RPSResult(Guid? winner) : this(winner, new Dictionary<Guid, RPSChoice>())
        {
        }

        [JsonConstructor]
        public RPSResult(Guid? winner, Dictionary<Guid, RPSChoice> dic)
        {
            Winner = winner;
            Signs = dic;
        }
    }
}
