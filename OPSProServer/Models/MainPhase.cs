using OPSProServer.Models;

public class MainPhase : IPhase
{
    public override PhaseType PhaseType => PhaseType.Main;

    public override bool IsActionAllowed(CardSource source, CardAction action)
    {
        return true;
    }


    public override IPhase NextPhase()
    {
        return new EndPhase();
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