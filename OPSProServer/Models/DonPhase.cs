using OPSProServer.Models;

public class DonPhase : IPhase
{
    public override PhaseType PhaseType => PhaseType.Don;

    public override bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }

    public override IPhase NextPhase()
    {
        return new MainPhase();
    }

    public override void OnPhaseEnded(Game game)
    {
    }

    public override void OnPhaseStarted(Game game)
    {
        var playerInfo = game.GetCurrentPlayerGameInformation();

        if (game.FirstToPlay == game.PlayerTurn && game.Turn == 1)
        {
            playerInfo.DrawDonCard(1);
        } else
        {
            playerInfo.DrawDonCard(2);
        }
    }

    public override bool IsAutoNextPhase()
    {
        return true;
    }
}
