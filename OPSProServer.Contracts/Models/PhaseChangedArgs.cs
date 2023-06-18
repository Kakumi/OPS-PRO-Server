using System;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class PhaseChangedArgs : EventArgs
    {
        public PhaseType OldPhaseType { get; }
        public PhaseType NewPhaseType { get; }
        public Game Game { get; }
        public TaskCompletionSource<bool> Task { get; }

        public PhaseChangedArgs(PhaseType oldPhaseType, PhaseType newPhaseType, Game game)
        {
            OldPhaseType = oldPhaseType;
            NewPhaseType = newPhaseType;
            Game = game;
            Task = new TaskCompletionSource<bool>();
        }
    }
}
