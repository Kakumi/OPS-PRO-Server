using OPSProServer.Models;

public class MainPhase : IPhase
{
    public PhaseType PhaseType => PhaseType.Main;

    public bool IsActionAllowed(CardSource source, CardAction action)
    {
        return true;
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