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
            try
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
                        }
                        else if (userId == room.Opponent.Id)
                        {
                            room.OpponentRPS = rps;
                        }

                        if (room.CreatorRPS != RockPaperScissors.None && room.OpponentRPS != RockPaperScissors.None)
                        {
                            var dic = new Dictionary<Guid, RockPaperScissors>();
                            dic.Add(room.Creator.Id, room.CreatorRPS);
                            dic.Add(room.Opponent.Id, room.OpponentRPS);

                            var winnerId = room.GetRockPaperScissorsWinner();

                            var result = new RockPaperScissorsResult()
                            {
                                Signs = dic,
                                Winner = winnerId
                            };

                            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.RPSExecuted), result);

                            if (winnerId != null)
                            {
                                var winnerUser = _userManager.GetUser(winnerId ?? Guid.Empty);
                                if (winnerUser == null)
                                {
                                    winnerUser = room.Creator;
                                }

                                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.ChooseFirstPlayerToPlay));
                            }
                            else
                            {
                                //If winner is null it's tie, reset values
                                room.CreatorRPS = RockPaperScissors.None;
                                room.OpponentRPS = RockPaperScissors.None;

#if DEBUG
                                room.OpponentRPS = RockPaperScissors.Rock;
#endif
                            }
                        }

                        return true;
                    }
                }

                return false;
            } catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> SetFirstPlayer(Guid userId, Guid firstToPlayId)
        {
            try
            {
                var user = _userManager.GetUser(userId);
                if (user != null)
                {
                    var room = _roomManager.GetRoom(user);
                    if (room != null && room.Opponent != null)
                    {
                        room.FirstToPlay = firstToPlayId;
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.FirstPlayerDecided), firstToPlayId);

                        return true;
                    }
                }

                return false;
            } catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> SyncBoard(Guid userId, PlaymatSync playmatSync)
        {
            try
            {
                var user = _userManager.GetUser(userId);
                if (user != null)
                {
                    var room = _roomManager.GetRoom(user);
                    if (room != null && room.Opponent != null && room.GetOpponent(userId) != null)
                    {
                        var opponent = room.GetOpponent(userId);
                        await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.SyncBoard), playmatSync);

#if DEBUG
                        await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.SyncBoard), new PlaymatSync()
                        {
                            UserId = opponent.Id,
                            Leader = Guid.NewGuid(),
                            Life = Guid.NewGuid(),
                            Deck = Guid.NewGuid(),
                            Stage = Guid.NewGuid(),
                            Trash = Guid.NewGuid(),
                            Cost = Guid.NewGuid(),
                            DonDeck = Guid.NewGuid(),
                            Characters = new List<Guid>()
                            {
                                Guid.NewGuid(),
                                Guid.NewGuid(),
                                Guid.NewGuid(),
                                Guid.NewGuid(),
                                Guid.NewGuid()
                            }
                        });
#endif

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
    }
}
