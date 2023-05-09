using Microsoft.AspNetCore.SignalR;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Hubs;

namespace OPS_Pro_Server.Hubs
{
    public partial class GameHub : Hub, IGameHub
    {
        protected readonly ILogger<GameHub> _logger;
        protected readonly IRoomManager _roomManager;
        protected readonly IUserManager _userManager;

        public GameHub(ILogger<GameHub> logger, IRoomManager roomManager, IUserManager userManager)
        {
            _logger = logger;
            _roomManager = roomManager;
            _userManager = userManager;
        }

        public async Task<bool> LaunchGame(Guid roomId)
        {
            try
            {
                var room = _roomManager.GetRoom(roomId);
                if (room != null && room.CanStart())
                {
                    _logger.LogInformation("Start duel for room {RoomId}", roomId);
                    room.State = RoomState.RockPaperScissors;
                    await Clients.Group(roomId.ToString()).SendAsync(nameof(IGameHubEvent.GameLaunched));
                    return true;
                }

                _logger.LogError("Can't start duel {RoomId} because room is null or cannot start.", roomId);
                return false;
            } catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> SetRockPaperScissors(Guid userId, RockPaperScissors rps)
        {
            var user = _userManager.GetUser(userId);
            if (user != null)
            {
                var room = _roomManager.GetRoom(user);
                if (room != null && room.Opponent != null)
                {
                    if (userId == room.Creator.Id)
                    {
                        room.CreatorRPS = rps;
                    } else if (userId == room.Opponent.Id)
                    {
                        room.OpponentRPS = rps;
                    }

                    if (room.CreatorRPS != RockPaperScissors.None && room.OpponentRPS != RockPaperScissors.None)
                    {
                        var dic = new Dictionary<Guid, RockPaperScissors>();
                        dic.Add(room.Creator.Id, room.CreatorRPS);
                        dic.Add(room.Opponent.Id, room.OpponentRPS);

                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.RPSExecuted), dic);
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
