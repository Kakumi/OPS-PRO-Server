using OPSProServer.Exceptions;
using OPSProServer.Models;

public class OpponentPhase : IPhase
{
    public override PhaseType PhaseType => PhaseType.Opponent;

    public override bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }


    public override IPhase NextPhase()
    {
        throw new UnauthorizedOperationException("update this phase");
    }

    public override void OnPhaseEnded(Game game)
    {
    }

    public override void OnPhaseStarted(Game game)
    {
    }

    public override bool IsAutoNextPhase()
    {
        return false;
    }
}
