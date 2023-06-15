namespace OPSProServer.Models
{
    public class Game
    {
        public Guid Id { get; }
        public GameState State { get; private set; }
        public int Turn { get; private set; }
        public Guid PlayerTurn { get; private set; }
        public Guid FirstToPlay { get; private set; }
        public PlayerGameInformation CreatorGameInformation { get; private set; }
        public PlayerGameInformation OpponentGameInformation { get; private set; }

        internal Game(Guid firstToPlay, PlayerGameInformation creatorGameInformation, PlayerGameInformation opponentGameInformation)
        {
            Id = Guid.NewGuid();
            State = GameState.Starting;
            Turn = 0;
            PlayerTurn = firstToPlay;
            FirstToPlay = firstToPlay;
            CreatorGameInformation = creatorGameInformation;
            OpponentGameInformation = opponentGameInformation;
        }

        public PlayerGameInformation GetCurrentPlayerGameInformation()
        {
            if (PlayerTurn == CreatorGameInformation.UserId)
            {
                return CreatorGameInformation;
            }

            return OpponentGameInformation;
        }

        public PlayerGameInformation GetMyPlayerInformation(Guid userId)
        {
            if (userId == CreatorGameInformation.UserId)
            {
                return CreatorGameInformation;
            }

            return OpponentGameInformation;
        }

        public PlayerGameInformation GetOpponentPlayerInformation(Guid userId)
        {
            if (userId == CreatorGameInformation.UserId)
            {
                return OpponentGameInformation;
            }

            return CreatorGameInformation;
        }
    }
}
