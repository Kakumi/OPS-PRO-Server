namespace OPSProServer.Models
{
    public class IPhase
    {
        public virtual PhaseType PhaseType => PhaseType.Don;
        public virtual void OnPhaseStarted(Game game) { }
        public virtual void OnPhaseEnded(Game game) { }
        public virtual bool IsActionAllowed(CardSource source, CardAction action) { return false; }
        public virtual IPhase NextPhase() { return null; }
        public virtual bool IsAutoNextPhase() { return false; }
    }
}
