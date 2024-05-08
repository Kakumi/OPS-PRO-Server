namespace OPSProServer.Contracts.Models
{
    public class MainPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.Main;

        public PhaseState State { get; set; }

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See ||
                source == CardSource.Leader ||
                source == CardSource.Character ||
                source == CardSource.Hand ||
                source == CardSource.Board ||
                source == CardSource.CostDeck ||
                source == CardSource.Deck ||
                source == CardSource.DonDeck ||
                source == CardSource.Life ||
                source == CardSource.Stage ||
                source == CardSource.Trash;
        }


        public IPhase NextPhase()
        {
            return new EndPhase();
        }

        public RuleResponse OnPhaseEnded(PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public RuleResponse OnPhaseStarted(PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public bool IsAutoNextPhase()
        {
            return false;
        }
    }
}