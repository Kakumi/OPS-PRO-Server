using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class Game
    {
        public Guid Id { get; }
        public GameState State { get; }
        public int Turn { get; set; }
        public Guid PlayerTurn { get; set; }
        public Guid FirstToPlay { get; set; }
        public PlayerGameInformation CreatorGameInformation { get; set; }
        public PlayerGameInformation OpponentGameInformation { get; set; }

        public Game(Guid firstToPlay, PlayerGameInformation creatorGameInformation, PlayerGameInformation opponentGameInformation)
        {
            Id = Guid.NewGuid();
            State = GameState.Starting;
            Turn = 0;
            PlayerTurn = firstToPlay;
            FirstToPlay = firstToPlay;
            CreatorGameInformation = creatorGameInformation;
            OpponentGameInformation = opponentGameInformation;
        }
    }
}
