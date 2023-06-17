using OPSProServer.Models;

public class EndPhase : IPhase
{
    public override PhaseType PhaseType => PhaseType.End;

    public override bool IsActionAllowed(CardSource source, CardAction action)
    {
        return action == CardAction.See;
    }

    public override IPhase NextPhase()
    {
        return new RefreshPhase(); //new OpponentPhase();
    }

    public override void OnPhaseEnded(Game game)
    {
        var playerInfo = game.GetCurrentPlayerGameInformation();

        playerInfo.GetCharacters().ForEach(x =>
        {
            x.RemoveStatDuration(ModifierDuration.Turn);
        });

        playerInfo.Leader.RemoveStatDuration(ModifierDuration.Turn);
    }

    public override void OnPhaseStarted(Game game)
    {
        //PlayerArea opponentArea;
        //if (playerArea.Gameboard.PlayerArea == playerArea)
        //{
        //    opponentArea = playerArea.Gameboard.OpponentArea;
        //} else
        //{
        //    opponentArea = playerArea.Gameboard.PlayerArea;
        //}

        //opponentArea.UpdatePhase(new DrawPhase());
    }

    public override bool IsAutoNextPhase()
    {
        return true;
    }
}