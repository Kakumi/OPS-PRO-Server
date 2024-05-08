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
        public PlayingCard? LifeCard { get; }
        public int AttackerPower { get; }
        public int DefenderPower { get; }
        public bool Success { get; }
        public bool Winner { get; }

        public AttackResult(PlayerGameInformation attackerGameInfo, PlayerGameInformation defenderGameInfo, PlayingCard attackerCard, PlayingCard defenderCard, PlayingCard? lifeCard, int attackerPower, int defenderPower, bool success, bool winner)
        {
            AttackerGameInfo = attackerGameInfo;
            DefenderGameInfo = defenderGameInfo;
            AttackerCard = attackerCard;
            DefenderCard = defenderCard;
            LifeCard = lifeCard;
            AttackerPower = attackerPower;
            DefenderPower = defenderPower;
            Success = success;
            Winner = winner;
        }
    }
}
