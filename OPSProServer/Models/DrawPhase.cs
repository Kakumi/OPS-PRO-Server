using OPSProServer.Models;

public class DrawPhase : IPhase
{
    public PhaseType PhaseType => PhaseType.Draw;

    public bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }

    public IPhase NextPhase()
    {
        return new DonPhase();
    }

    public void OnPhaseEnded(Game game)
    {
    }

    public void OnPhaseStarted(Game game)
    {
        var playerInfo = game.GetCurrentPlayerGameInformation();

        if (game.FirstToPlay != playerInfo.UserId || game.Turn != 1)
        {
            playerInfo.DrawCard();
        }
    }

    public bool IsAutoNextPhase()
    {
        return true;
    }
}