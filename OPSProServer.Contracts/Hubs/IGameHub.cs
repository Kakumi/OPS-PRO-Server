using OPSProServer.Contracts.Models;
using System;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Hubs
{
    public interface IGameHub
    {
        Task<bool> LaunchRockPaperScissors(Guid roomId);
        Task<bool> SetRockPaperScissors(Guid userId, RPSChoice rps);
        Task<bool> LaunchGame(Guid userId, Guid userToStart);
    }
}
