using System;

namespace OPSProServer.Contracts.Models
{
    public interface IPhase
    {
        PhaseState State { get; set; }
        PhaseType PhaseType { get; }
        RuleResponse OnPhaseStarted(PlayerGameInformation gameInfo, Game game);
        RuleResponse OnPhaseEnded(PlayerGameInformation gameInfo, Game game);
        bool IsActionAllowed(CardSource source, CardAction action);
        IPhase NextPhase();
        bool IsAutoNextPhase();
    }
}
