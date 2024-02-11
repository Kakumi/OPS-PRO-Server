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

        public GameHub(ILogger<GameHub> logger, ICardService cardService, IRoomManager roomManager, IUserManager userManager, IFlowManager resolverManager)
        {
            _logger = logger;
            _cardService = cardService;
            _roomManager = roomManager;
            _userManager = userManager;
            _resolverManager = resolverManager;
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
            var ruleResponse = new RuleResponse();
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;

            if (room!.Opponent != null)
            {
                var userStart = _userManager.GetUser(userToStart)!;
                var game = room.StartGame(userToStart);

                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.GameStarted), userToStart);
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), game);
                await Clients.Client(room.Opponent.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_VERSUS_START", user.Username));
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_VERSUS_START", room.Opponent.Username));
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_USER_START", userStart.Username));

                return await ManageFlowAction(user, room, ruleResponse);
            }

            return false;
        }

        [Connected(true, true, true)]
        public async Task<bool> NextPhase()
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;

            var response = room!.Game!.CheckUpdatePhaseState(true);

            if (response != null)
            {
                await ManageFlowAction(user!, room, response);
            }

            return true;
        }

        [Connected(true, true, true)]
        public async Task<bool> GetAttackableCards(Guid attacker)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var myGameInfo = room!.Game!.GetMyPlayerInformation(user!.Id);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(user.Id);
            var result = room.Game.CanAttack(myGameInfo.GetCard(attacker), user);

            if (result.Success)
            {
                var cards = room.Game.GetAttackableCards(user);

                var flowAction = new FlowAction(user, user, ResolveAttackable);
                var flowRequest = new FlowActionRequest(flowAction.Id, user, "GAME_CHOOSE_ATTACK_OPPONENT", cards.Ids().ToList(), 1, 1, true);
                flowAction.Request = flowRequest;
                flowAction.FromCardId = attacker;
                flowAction.ToCardId = null;
                flowAction.FinalContext = FlowContext.Attack;

                return await ManageFlowAction(user, room, flowAction);
            }

            return true;
        }

        private RuleResponse ResolveAttackable(FlowArgs args)
        {
            return new RuleResponse();
        }

        [Connected(true, true, true)]
        public async Task<bool> Attack(Guid attacker, Guid target)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var myGameInfo = room!.Game!.GetMyPlayerInformation(user!.Id);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(user.Id);
            var result = room.Game.CanAttack(myGameInfo.GetCard(attacker), user);

            if (result.Success)
            {
                var attackerCard = myGameInfo.GetCharacterOrLeader(attacker);
                var defenderCard = opponentGameInfo.GetCharacterOrLeader(target);
                if (attackerCard != null && defenderCard != null)
                {
                    var attackFlow = PrepareAttackCheckOpponentBlockers(user.Id, attacker, target);
                    attackFlow.AddLast(PrepareAttackCheckOpponentCounters(user.Id, attacker, target));

                    return await ManageFlowAction(user, room, attackFlow);
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

            var response = room.Game.GiveDon(user, characterCardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);

            return await ManageFlowAction(user!, room, response);
        }

        [Connected(true, true, true)]
        public async Task<bool> Summon(Guid cardId)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var gameInfo = room!.Game!.GetCurrentPlayerGameInformation();
            var response = room.Game.Summon(user!, cardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);

            return await ManageFlowAction(user!, room, response);
        }

        [Connected(true, true, true)]
        public Task<bool> ActivateCardEffect(Guid characterCardId)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> ManageFlowAction(User user, Room room, RuleResponse ruleResponse)
        {
            foreach(var flowMessage in ruleResponse.FlowResponses) {
                await SendFlowMessage(room, flowMessage);
            }

            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game!);

            if (ruleResponse.FlowAction != null)
            {
                return await ManageFlowAction(user, room, ruleResponse.FlowAction);
            }

            var newResponse = room.Game!.CheckUpdatePhaseState(false);
            if (newResponse != null)
            {
                return await ManageFlowAction(user, room, newResponse);
            }

            return true;
        }

        private async Task<bool> ManageFlowAction(User user, Room room, FlowAction action)
        {
            if (action.CanExecute(user, room, room.Game!))
            {
                var opponent = room.GetOpponent(action.Request!.UserTarget);
                if (room.Game!.PlayerTurn == opponent!.Id)
                {
                    var opponentGameInfo = room.Game.GetOpponentPlayerInformation(action.Request!.UserTarget.Id);
                    opponentGameInfo.Waiting = true;
                    await Clients.Client(opponent.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), opponentGameInfo.Waiting);
                }

                _resolverManager.Add(action);
                await Clients.Client(action.Request!.UserTarget.ConnectionId).SendAsync(nameof(IGameHubEvent.FlowActionRequest), action.Request!);
                return true;
            }

            RuleResponse? contextResponse = await RunFinalContext(action, action.NextAction, null);
            if (contextResponse != null)
            {
                if (action.NextAction != null)
                {
                    if (contextResponse.FlowAction == null)
                    {
                        contextResponse.FlowAction = action.NextAction;
                    }
                    else
                    {
                        contextResponse.FlowAction.AddLast(action.NextAction);
                    }
                }

                return await ManageFlowAction(user, room, contextResponse);
            }


            if (action.NextAction != null)
            {
                return await ManageFlowAction(user, room, action.NextAction);
            }

            return true;
        }

        [Connected(true, true)]
        public async Task<bool> ResolveFlow(FlowActionResponse response)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;
            var flow = _resolverManager.Get(response.FlowId);

            if (flow != null && (flow.FromUser.Id == user!.Id || flow.ToUser.Id == user!.Id))
            {
                var opponent = room!.GetOpponent(flow.Request!.UserTarget);
                var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(flow.Request!.UserTarget.Id);
                opponentGameInfo.Waiting = false;

                await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), opponentGameInfo.Waiting);

                var ruleResponse = flow.Action(new FlowArgs(user!, room!, room!.Game!, flow, response));

                var nextFlow = _resolverManager.Resolve(flow.Id);
                if (nextFlow != null) 
                {
                    if (ruleResponse.FlowAction != null)
                    {
                        if (ruleResponse.PriorityFlowAction)
                        {
                            nextFlow.AddFirst(ruleResponse.FlowAction);
                        }
                        else
                        {
                            nextFlow.AddLast(ruleResponse.FlowAction);
                        }
                    }

                    ruleResponse.FlowAction = nextFlow;
                }

                RuleResponse? contextResponse = await RunFinalContext(flow, ruleResponse.FlowAction, response);
                if (contextResponse != null)
                {
                    ruleResponse.Add(contextResponse);
                }

                while (ruleResponse.FlowAction != null && !ruleResponse.FlowAction.CanExecute(user, room, room.Game))
                {
                    RuleResponse? customResponse = await RunFinalContext(flow, ruleResponse.FlowAction, response);
                    if (customResponse != null)
                    {
                        ruleResponse.Add(customResponse);
                    }

                    ruleResponse.FlowAction = ruleResponse.FlowAction.NextAction;
                }

                return await ManageFlowAction(user, room, ruleResponse);
            }

            return false;
        }

        private async Task SendFlowMessage(Room room, FlowResponseMessage? message)
        {
            if (message != null)
            {
                if (message.User != null)
                {
                    await Clients.Client(message.User!.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), message.UserGameMessage);
                }
                else
                {
                    await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), message.UserGameMessage);
                }
            }
        }

        private async Task<RuleResponse?> RunFinalContext(FlowAction currentAction, FlowAction? nextAction, FlowActionResponse? response)
        {
            if (nextAction == null || nextAction.FinalContext != currentAction.FinalContext)
            {
                switch (currentAction.FinalContext)
                {
                    case FlowContext.None:
                        break;
                    case FlowContext.Attack:
                        if (response != null && response.CardsId.Count > 0 && currentAction.FromCardId != null)
                        {
                            await Attack(currentAction.FromCardId!.Value, response.CardsId.First());
                        }

                        break;
                    case FlowContext.ResolveAttack:
                        if (currentAction.FromCardId != null && currentAction.ToCardId != null)
                        {
                            return await ResolveAttack(currentAction.FromUser.Id, currentAction.FromCardId!.Value, currentAction.ToCardId!.Value);
                        }

                        break;
                }
            }

            return null;
        }
    }
}
