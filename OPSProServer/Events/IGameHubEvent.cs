using OPSProServer.Models;

namespace OPSProServer.Events
{
    public interface IGameHubEvent
    {
        void RockPaperScissorsStarted();
        RPSResult RPSExecuted();
        void ChooseFirstPlayerToPlay();
        Guid GameStarted();
        Game BoardUpdated();
    }
}
