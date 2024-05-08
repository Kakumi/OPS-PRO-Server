using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class RPSResult
    {
        public Guid CreatorId { get; set; }
        public RPSChoice CreatorChoice { get; set; }
        public Guid OpponentId { get; set; }
        public RPSChoice OpponentChoice { get; set; }
        public Guid? Winner { get; }

        [JsonConstructor]
        public RPSResult(Guid creatorId, RPSChoice creatorChoice, Guid opponentId, RPSChoice opponentChoice, Guid? winner)
        {
            CreatorId = creatorId;
            CreatorChoice = creatorChoice;
            OpponentId = opponentId;
            OpponentChoice = opponentChoice;
            Winner = winner;
        }

        public RPSChoice GetOpponentChoice(Guid id)
        {
            if (CreatorId == id)
            {
                return OpponentChoice;
            }

            return CreatorChoice;
        }
    }
}
