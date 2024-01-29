using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class AttackResult
    {
        public PlayerGameInformation AttackerGameInfo { get; }
        public PlayerGameInformation DefenderGameInfo { get; }
        public PlayingCard AttackerCard { get; }
        public PlayingCard DefenderCard { get; }
        public bool Success { get; }

        public AttackResult(PlayerGameInformation attackerGameInfo, PlayerGameInformation defenderGameInfo, PlayingCard attackerCard, PlayingCard defenderCard, bool success)
        {
            AttackerGameInfo = attackerGameInfo;
            DefenderGameInfo = defenderGameInfo;
            AttackerCard = attackerCard;
            DefenderCard = defenderCard;
            Success = success;
        }
    }
}
