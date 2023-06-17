using OPSProServer.Models;

public class RefreshPhase : IPhase
{
    public override PhaseType PhaseType => PhaseType.Refresh;

    public override bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }

    public override IPhase NextPhase()
    {
        return new DrawPhase();
    }

    public override void OnPhaseEnded(Game game)
    {
    }

    public override void OnPhaseStarted(Game game)
    {
        var playerInfo = game.GetCurrentPlayerGameInformation();

        if (game.FirstToPlay == game.PlayerTurn)
        {
            game.IncrementTurn();
        }

        playerInfo.UnrestCostDeck();
        playerInfo.GetCharacters().ForEach(x =>
        {
            x.Rested = false;
            x.RemoveStatDuration(ModifierDuration.OpponentTurn);
        });

        playerInfo.Leader.Rested = false;
        playerInfo.Leader.RemoveStatDuration(ModifierDuration.OpponentTurn);
    }

    public override bool IsAutoNextPhase()
    {
        return true;
    }
}
