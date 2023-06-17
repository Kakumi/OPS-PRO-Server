using OPSProServer.Models;

public class DrawPhase : IPhase
{
    public override PhaseType PhaseType => PhaseType.Draw;

    public override bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }

    public override IPhase NextPhase()
    {
        return new DonPhase();
    }

    public override void OnPhaseEnded(Game game)
    {
    }

    public override void OnPhaseStarted(Game game)
    {
        var playerInfo = game.GetCurrentPlayerGameInformation();

        if (game.FirstToPlay != playerInfo.UserId || game.Turn != 1)
        {
            playerInfo.DrawCard();
        }
    }

    public override bool IsAutoNextPhase()
    {
        return true;
    }
}