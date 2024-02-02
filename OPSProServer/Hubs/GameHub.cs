using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Events;
using OPSProServer.Managers;
using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Exceptions;
using OPSProServer.Attributes;
using OPSProServer.Services;
using OPSProServer.Models;
using System.ComponentModel;

namespace OPSProServer.Hubs
{
    public partial class GameHub : Hub, IGameHub
    {
        protected readonly ILogger<GameHub> _logger;
        protected readonly ICardService _cardService;
        protected readonly IRoomManager _roomManager;
        protected readonly IUserManager _userManager;
        protected readonly IFlowManager _resolverManager;
        protected readonly IGameRuleService _gameRuleService;

        public GameHub(ILogger<GameHub> logger, ICardService cardService, IRoomManager roomManager, IUserManager userManager, IFlowManager resolverManager, IGameRuleService gameRuleService)
        {
            _logger = logger;
            _cardService = cardService;
            _roomManager = roomManager;
            _userManager = userManager;
            _resolverManager = resolverManager;
            _gameRuleService = gameRuleService;
        }

        [Connected(true)]
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

        [Connected(true)]
        public async Task<bool> SetRockPaperScissors(RPSChoice rps)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;

            if (room!.Opponent != null)
            {
                if (user!.Id == room.Creator.Id)
                {
                    room.Creator.RPSChoice = rps;
                }
                else if (user.Id == room.Opponent.Id)
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

            return false;
        }

        [Connected(true)]
        public async Task<bool> LaunchGame(Guid userToStart)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;

            if (room!.Opponent != null)
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

            return false;
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

        [Connected(true, true, true)]
        public async Task<bool> NextPhase()
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;

            room!.Game!.PhaseChanged += Game_PhaseChanged;
            await room.Game.UpdatePhase();
            room.Game.PhaseChanged -= Game_PhaseChanged;

            return true;
        }

        [Connected(true, true, true)]
        public async Task<bool> GetAttackableCards(Guid attacker)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var myGameInfo = room!.Game!.GetMyPlayerInformation(user!.Id);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(user.Id);

            if (_gameRuleService.CanAttack(myGameInfo.GetCard(attacker), user, room, room.Game))
            {
                var cards = _gameRuleService.GetAttackableCards(user, room, room.Game);
                var result = new AttackableResult(attacker, cards.Ids());
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.AttackableCards), result);
            }

            return true;
        }

        [Connected(true, true, true)]
        public async Task<bool> Attack(Guid attacker, Guid target)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var myGameInfo = room!.Game!.GetMyPlayerInformation(user!.Id);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(user.Id);

            if (_gameRuleService.CanAttack(myGameInfo.GetCard(attacker), user, room, room.Game))
            {
                var attackerCard = myGameInfo.GetCharacterOrLeader(attacker);
                var defenderCard = opponentGameInfo.GetCharacterOrLeader(target);
                if (attackerCard != null && defenderCard != null)
                {
                    var blockerFlow = PrepareAttackCheckOpponentBlockers(user.Id, attacker, target);
                    if (blockerFlow != null)
                    {
                        return await ManageFlowAction(user, room, blockerFlow);
                    }

                    var counterFlow = PrepareAttackCheckOpponentCounters(user.Id, attacker, target);
                    if (counterFlow != null)
                    {
                        return await ManageFlowAction(user, room, counterFlow);
                    }

                    return await ResolveAttack(user.Id, attacker, target);
                }
            }

            throw new ErrorUserActionException(user.Id, "GAME_CARD_NOT_FOUND");
        }

        [Connected(true, true, true)]
        public async Task<bool> GiveDonCard(Guid characterCardId)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var gameInfo = room!.Game!.GetMyPlayerInformation(user!.Id);

            var response = _gameRuleService.GiveDon(user, room, room.Game, characterCardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);

            foreach (var message in response.FlowResponses)
            {
                await SendFlowMessage(message);
            }

            return true;
        }

        [Connected(true, true, true)]
        public async Task<bool> Summon(Guid cardId)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var gameInfo = room!.Game!.GetCurrentPlayerGameInformation();
            var response = _gameRuleService.Summon(user!, room, room.Game, cardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);
            foreach(var message in response.FlowResponses)
            {
                await SendFlowMessage(message);
            }

            return true;
        }

        [Connected(true, true, true)]
        public Task<bool> ActivateCardEffect(Guid characterCardId)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> ManageFlowAction(User user, Room room, FlowAction action)
        {
            var opponent = room.GetOpponent(action.Request!.UserTarget);
            var opponentGameInfo = room.Game.GetOpponentPlayerInformation(action.Request!.UserTarget.Id);
            opponentGameInfo.Waiting = true;

            _resolverManager.AddFlow(action);

            await Clients.Client(opponent.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), opponentGameInfo.Waiting);
            await Clients.Client(action.Request!.UserTarget.ConnectionId).SendAsync(nameof(IGameHubEvent.FlowActionRequest), action.Request!);

            return true;
        }

        [Connected(true, true)]
        public async Task<bool> ResolveFlow(FlowActionResponse response)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var flow = _resolverManager.GetFlow(response.FlowId);

            if (flow != null && (flow.FromUser.Id == user!.Id || flow.ToUser.Id == user!.Id))
            {
                var opponent = room!.GetOpponent(flow.Request!.UserTarget);
                var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(flow.Request!.UserTarget.Id);
                opponentGameInfo.Waiting = false;

                await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), opponentGameInfo.Waiting);

                var messages = flow.Action(new FlowArgs(user!, room!, room!.Game!, flow, response));
                foreach (var message in messages)
                {
                    await SendFlowMessage(message);
                }

                var nextFlow = _resolverManager.ResolveFlow(flow.Id);
                if (nextFlow != null)
                {
                    await ManageFlowAction(user!, room!, nextFlow);
                }

                await Clients.Group(room!.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);

                return true;
            }

            return false;
        }

        private async Task SendFlowMessage(FlowResponseMessage? message)
        {
            if (message != null)
            {
                if (message.User != null)
                {
                    await Clients.Client(message.User!.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), message.UserGameMessage);
                }
                else if (message.Room != null)
                {
                    await Clients.Group(message.Room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), message.UserGameMessage);
                }
            }
        }

        [Connected(true, true)]
        public async Task<bool> Test()
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;

            _ = Task.Run(async () => await TestRunner());

            var flowAction1 = new FlowAction(user, user, Test1);
            var request1 = new FlowActionRequest(flowAction1.Id, user, "TEST1_CODE", false);
            flowAction1.Request = request1;

            var flowAction2 = new FlowAction(user, user, Test2);
            var request2 = new FlowActionRequest(flowAction2.Id, user, "TEST2_CODE", false);
            flowAction2.Request = request2;

            flowAction1.AddLast(flowAction2);

            return await ManageFlowAction(user, room!, flowAction1);
        }

        public List<FlowResponseMessage> Test1(FlowArgs args)
        {
            return new List<FlowResponseMessage>()
            {
                new FlowResponseMessage(args.User, new UserGameMessage("HELLO_1"))
            };
        }

        public List<FlowResponseMessage> Test2(FlowArgs args)
        {
            return new List<FlowResponseMessage>()
            {
                new FlowResponseMessage(args.User, new UserGameMessage("HELLO_2"))
            };
        }

        public async Task TestRunner()
        {
            await Task.Delay(500);
            var user = Context.Items["user"] as User;
            await Clients.Client(user!.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("RUNNER"));
        }
    }
}
