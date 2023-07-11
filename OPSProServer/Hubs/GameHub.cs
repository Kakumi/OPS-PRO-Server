using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Events;
using OPSProServer.Managers;
using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Hubs;

namespace OPSProServer.Hubs
{
    internal partial class GameHub : Hub, IGameHub
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

        public async Task<bool> LaunchRockPaperScissors(Guid roomId)
        {
            try
            {
                var room = _roomManager.GetRoom(roomId);
                if (room != null && room.CanStart())
                {
                    _logger.LogInformation("Start duel for room {RoomId}", roomId);
                    await Clients.Group(roomId.ToString()).SendAsync(nameof(IGameHubEvent.RockPaperScissorsStarted));
                    return true;
                }

                _logger.LogError("Can't start duel {RoomId} because room is null or cannot start.", roomId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> SetRockPaperScissors(Guid userId, RPSChoice rps)
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
                            room.Creator.RPSChoice = rps;
                        }
                        else if (userId == room.Opponent.Id)
                        {
                            room.Opponent.RPSChoice = rps;
                        }

                        if (room.Creator.RPSChoice != RPSChoice.None && room.Opponent.RPSChoice != RPSChoice.None)
                        {
                            var result = room.GetRPSWinner();

                            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.RPSExecuted), result);

                            if (result.Winner != null)
                            {
                                var winnerUser = _userManager.GetUser(result.Winner ?? Guid.Empty);
                                if (winnerUser == null)
                                {
                                    winnerUser = room.Creator;
                                }

                                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.ChooseFirstPlayerToPlay));
                            }
                            else
                            {
                                //If winner is null it's tie, reset values
                                room.Creator.RPSChoice = RPSChoice.None;
                                room.Opponent.RPSChoice = RPSChoice.None;

#if DEBUG
                                room.Opponent.RPSChoice = RPSChoice.Paper;
#endif
                            }
                        }

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

        public async Task<bool> LaunchGame(Guid userId, Guid userToStart)
        {
            try
            {
                var user = _userManager.GetUser(userId);
                if (user != null)
                {
                    var room = _roomManager.GetRoom(user);
                    if (room != null && room.Opponent != null)
                    {
                        var game = room.StartGame(userToStart);
                        game.PhaseChanged += Game_PhaseChanged;

                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.GameStarted), userToStart);
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), game);

                        if (game.GetCurrentPlayerGameInformation().CurrentPhase!.IsAutoNextPhase())
                        {
                            await game.UpdatePhase();
                        }

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

        private async void Game_PhaseChanged(object? sender, PhaseChangedArgs e)
        {
            try
            {
                var user = _userManager.GetUser(e.Game.PlayerTurn);
                if (user != null)
                {
                    var room = _roomManager.GetRoom(user);

                    if (room != null)
                    {
                        if (e.NewPhaseType == PhaseType.Draw)
                        {

                        }

                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), e.Game);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            } finally
            {
                e.WaitCompletion.SetResult(true);
            }
        }

        public async Task<bool> NextPhase(Guid userId)
        {
            try
            {
                var user = _userManager.GetUser(userId);
                if (user != null)
                {
                    var room = _roomManager.GetRoom(user);
                    if (room != null && room.Opponent != null && room.Game != null)
                    {
                        if (room.Game.PlayerTurn == userId)
                        {
                            await room.Game.UpdatePhase();
                        }
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
