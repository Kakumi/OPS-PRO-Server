using OPSProServer.Models;

public class DonPhase : IPhase
{
    public PhaseType PhaseType => PhaseType.Don;

    public bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }

    public IPhase NextPhase()
    {
        return new MainPhase();
    }

    public void OnPhaseEnded(Game game)
    {
    }

    public void OnPhaseStarted(Game game)
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

    public bool IsAutoNextPhase()
    {
        return true;
    }
}
