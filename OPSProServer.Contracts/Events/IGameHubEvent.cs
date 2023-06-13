using OPSProServer.Contracts.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Events
{
    public interface IGameHubEvent
    {
        void GameLaunched();
        RPSResult RPSExecuted();
        void ChooseFirstPlayerToPlay();
        Guid FirstPlayerDecided();
    }
}
