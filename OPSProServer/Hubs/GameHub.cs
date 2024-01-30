using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Events;
using OPSProServer.Managers;
using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Exceptions;
using OPSProServer.Attributes;
using OPSProServer.Services;

namespace OPSProServer.Hubs
{
    public partial class GameHub : Hub, IGameHub
    {
        protected readonly ILogger<GameHub> _logger;
        protected readonly ICardService _cardService;
        protected readonly IRoomManager _roomManager;
        protected readonly IUserManager _userManager;
        protected readonly IResolverManager _resolverManager;
        protected readonly IGameRuleService _gameRuleService;

        public GameHub(ILogger<GameHub> logger, ICardService cardService, IRoomManager roomManager, IUserManager userManager, IResolverManager resolverManager, IGameRuleService gameRuleService)
        {
            _logger = logger;
            _cardService = cardService;
            _roomManager = roomManager;
            _userManager = userManager;
            _resolverManager = resolverManager;
            _gameRuleService = gameRuleService;
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

                            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.RPSExecuted), result!);

                            if (result.Winner != null)
                            {
                                var winnerUser = _userManager.GetUser(result.Winner ?? Guid.Empty);
                                if (winnerUser == null)
                                {
                                    winnerUser = room.Creator;
                                }

                                await Clients.Client(winnerUser.ConnectionId).SendAsync(nameof(IGameHubEvent.ChooseFirstPlayerToPlay));
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
                        var userStart = _userManager.GetUser(userToStart)!;
                        var game = room.StartGame(userToStart);
                        game.PhaseChanged += Game_PhaseChanged;

                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.GameStarted), userToStart);
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), game);
                        await Clients.Client(room.Opponent.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_VERSUS_START", user.Username));
                        await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_VERSUS_START", room.Opponent.Username));
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_USER_START", userStart.Username));

                        if (game.GetCurrentPlayerGameInformation().CurrentPhase!.IsAutoNextPhase())
                        {
                            await game.UpdatePhase();
                        }

                        game.PhaseChanged -= Game_PhaseChanged;

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

        //We need to subscribe -> execute -> unsubscribe because:
        //When the first called is made (launch game) the event is subscribed on the right context (not disposed)
        //But for a second called for "NextPhase", the event will use the context it was registered on (disposed at this time)
        //So, we need to remove the subscriber after all calls and then subcribe it again before each calls.
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
                        var opponent = room.Opponent;
                        if (e.NewPhaseType == PhaseType.Opponent && e.OldPhaseType == PhaseType.End)
                        {
                            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("PLAYER_END_TURN", user.Username));
                            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("PLAYER_START_TURN", opponent!.Username));
                            await e.Game.NextPlayer();
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

        [InGame]
        public async Task<bool> NextPhase(Guid userId)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            room!.Game!.PhaseChanged += Game_PhaseChanged;
            await room.Game.UpdatePhase();
            room.Game.PhaseChanged -= Game_PhaseChanged;

            return true;
        }

        [InGame]
        public async Task<bool> GetAttackableCards(Guid userId, Guid attacker)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var myGameInfo = room.Game!.GetMyPlayerInformation(userId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userId);

            await CheckCounters(userId, myGameInfo.Leader.Id, opponentGameInfo.Leader.Id, null);

            if (_gameRuleService.CanAttack(myGameInfo.GetCard(attacker), user, room.Game))
            {
                var cards = _gameRuleService.GetAttackableCards(user, room.Game);
                var result = new AttackableResult(attacker, cards.Ids());
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.AttackableCards), result);
            }

            return true;
        }

        [InGame]
        public async Task<bool> Attack(Guid userId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var myGameInfo = room.Game!.GetMyPlayerInformation(userId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userId);

            if (_gameRuleService.CanAttack(myGameInfo.GetCard(attacker), user, room.Game))
            {
                var attackerCard = myGameInfo.GetAttacker(attacker);
                var defenderCard = opponentGameInfo.GetAttacker(target);
                if (attackerCard != null && defenderCard != null)
                {
                    var sent = await CheckBlockers(userId, attacker, target);
                    if (sent)
                    {
                        return true;
                    }

                    sent = await CheckCounters(userId, attacker, target, null);
                    if (sent)
                    {
                        return true;
                    }

                    return await ResolveAttack(userId, attacker, target);
                }
            }

            throw new ErrorUserActionException(userId, "GAME_CARD_NOT_FOUND");
        }

        [InGame]
        public async Task<bool> GiveDonCard(Guid userId, Guid characterCardId)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var gameInfo = room!.Game!.GetMyPlayerInformation(userId);

            var response = _gameRuleService.GiveDon(user, room.Game, characterCardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);

            foreach (var message in response.CodesMessage)
            {
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), message);
            }

            return true;
        }

        [InGame]
        public async Task<bool> Summon(Guid userId, Guid cardId)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var gameInfo = room!.Game!.GetCurrentPlayerGameInformation();
            var response = _gameRuleService.Summon(user, room.Game, cardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);
            foreach(var message in response.CodesMessage)
            {
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), message);
            }

            return true;
        }

        [InGame]
        public Task<bool> ActivateCardEffect(Guid userId, Guid characterCardId)
        {
            throw new NotImplementedException();
        }
    }
}
