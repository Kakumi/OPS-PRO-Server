using OPSProServer.Contracts.Exceptions;

namespace OPSProServer.Contracts.Models
{
    public class OpponentPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.Opponent;

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See;
        }


        public IPhase NextPhase()
        {
            throw new UnauthorizedOperationException("update this phase");
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
