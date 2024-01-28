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
            return new OpponentPhase();
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

        }

        public bool IsAutoNextPhase()
        {
            return true;
        }
    }
}