using OPSProServer.Contracts.Exceptions;

namespace OPSProServer.Contracts.Models
{
    public class OpponentPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.Opponent;

        public PhaseState State { get; set; }

        public OpponentPhase(bool playFirst)
        {
            if (playFirst)
            {
                State = PhaseState.Ending;
            } else
            {
                State = PhaseState.Beginning;
            }
        }

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See;
        }


        public IPhase NextPhase()
        {
            return new RefreshPhase();
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
