using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class RPSGame
    {
        public RPSPlayer Creator { get; set; }
        public RPSPlayer Opponent { get; set; }

        public RPSGame(Guid creator, Guid opponent)
        {
            Creator = new RPSPlayer(creator);
            Opponent = new RPSPlayer(opponent);
        }

        public Guid? GetWinner()
        {
            if (Creator.Choice != RPSChoice.None && Opponent.Choice != RPSChoice.None)
            {
                // Define the relationships between moves
                int[,] relationships = { { 0, -1, 1 }, { 1, 0, -1 }, { -1, 1, 0 } };

                // Determine winner
                int creatorIndex = (int)Creator.Choice - 1;
                int opponentIndex = (int)Opponent.Choice - 1;
                int result = relationships[creatorIndex, opponentIndex];

                // Return winner id
                if (result == 1)
                {
                    return Creator.UserId;
                }
                else if (result == -1)
                {
                    return Opponent.UserId;
                }
            }

            return null;
        }
    }
}
