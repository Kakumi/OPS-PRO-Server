using OPSProServer.Models;

public class RefreshPhase : IPhase
{
    public PhaseType PhaseType => PhaseType.Refresh;

    public bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }

    public IPhase NextPhase()
    {
        return new DrawPhase();
    }

    public void OnPhaseEnded(Game game)
    {
    }

    public void OnPhaseStarted(Game game)
    {
        var playerInfo = game.GetCurrentPlayerGameInformation();

        if (game.FirstToPlay == game.PlayerTurn)
        {
            game.IncrementTurn();
        }

        playerInfo.UnrestCostDeck();
        playerInfo.Characters.ForEach(x =>
        {
            x.Rested = false;
            x.RemoveStatDuration(ModifierDuration.OpponentTurn);
        });

        playerInfo.Leader.Rested = false;
        playerInfo.Leader.RemoveStatDuration(ModifierDuration.OpponentTurn);
    }

    public bool IsAutoNextPhase()
    {
        return true;
    }
}
