namespace OPSProServer.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public GameState State { get; set; }
        public int Turn { get; set; }
        public Guid PlayerTurn { get; set; }
        public Guid FirstToPlay { get; set; }
        public PlayerGameInformation CreatorGameInformation { get; set; }
        public PlayerGameInformation OpponentGameInformation { get; set; }
        public event EventHandler<PhaseChangedArgs>? PhaseChanged;

        public Game()
        {

        }

        public Game(Guid firstToPlay, PlayerGameInformation creatorGameInformation, PlayerGameInformation opponentGameInformation)
        {
            Id = Guid.NewGuid();
            State = GameState.Starting;
            Turn = 0;
            PlayerTurn = firstToPlay;
            FirstToPlay = firstToPlay;
            CreatorGameInformation = creatorGameInformation;
            OpponentGameInformation = opponentGameInformation;

            CreatorGameInformation.CurrentPhase.OnPhaseStarted(this);
            OpponentGameInformation.CurrentPhase.OnPhaseStarted(this);
        }

        public async Task UpdatePhase()
        {
            var currentPlayerInfo = GetCurrentPlayerGameInformation();
            var oldPhase = currentPlayerInfo.CurrentPhase;
            var newPhase = currentPlayerInfo.CurrentPhase.NextPhase();

            currentPlayerInfo.CurrentPhase.OnPhaseEnded(this);
            currentPlayerInfo.CurrentPhase = newPhase;
            currentPlayerInfo.CurrentPhase.OnPhaseStarted(this);

            if (PhaseChanged != null)
            {
                var args = new PhaseChangedArgs(oldPhase.PhaseType, newPhase.PhaseType, this);
                PhaseChanged.Invoke(this, args);
                await args.Task.Task;
            }

            if (currentPlayerInfo.CurrentPhase.IsAutoNextPhase())
            {
                await UpdatePhase();
            }
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

        public void IncrementTurn()
        {
            Turn++;
        }
    }
}
