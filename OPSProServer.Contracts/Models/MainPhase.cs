namespace OPSProServer.Contracts.Models
{
    public class MainPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.Main;

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

        public void OnPhaseEnded(Game game)
        {
        }

        public void OnPhaseStarted(Game game)
        {
        }

        public bool IsAutoNextPhase()
        {
            return false;
        }
    }
}