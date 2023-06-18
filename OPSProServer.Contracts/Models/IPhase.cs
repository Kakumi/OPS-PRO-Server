using System;

namespace OPSProServer.Contracts.Models
{
    public interface IPhase
    {
        PhaseType PhaseType { get; }
        void OnPhaseStarted(Game game);
        void OnPhaseEnded(Game game);
        bool IsActionAllowed(CardSource source, CardAction action);
        IPhase NextPhase();
        bool IsAutoNextPhase();
    }
}
