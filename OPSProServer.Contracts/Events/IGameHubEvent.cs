using OPSProServer.Contracts.Models;
using System;

namespace OPSProServer.Contracts.Events
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
