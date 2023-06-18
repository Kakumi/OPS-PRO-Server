namespace OPSProServer.Contracts.Models
{
    public class EndPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.End;

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See;
        }

        public IPhase NextPhase()
        {
            return new RefreshPhase(); //new OpponentPhase();
        }

        public void OnPhaseEnded(Game game)
        {
            var playerInfo = game.GetCurrentPlayerGameInformation();

            playerInfo.GetCharacters().ForEach(x =>
            {
                x.RemoveStatDuration(ModifierDuration.Turn);
            });

            playerInfo.Leader.RemoveStatDuration(ModifierDuration.Turn);
        }

        public void OnPhaseStarted(Game game)
        {
            //PlayerArea opponentArea;
            //if (playerArea.Gameboard.PlayerArea == playerArea)
            //{
            //    opponentArea = playerArea.Gameboard.OpponentArea;
            //} else
            //{
            //    opponentArea = playerArea.Gameboard.PlayerArea;
            //}

            //opponentArea.UpdatePhase(new DrawPhase());
        }

        public bool IsAutoNextPhase()
        {
            return true;
        }
    }
}