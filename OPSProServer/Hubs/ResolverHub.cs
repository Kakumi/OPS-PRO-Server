using Microsoft.AspNetCore.SignalR;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;
using System.Collections.Generic;

namespace OPSProServer.Hubs
{
    public partial class GameHub : Hub, IResolverHub
    {
        private FlowAction? PrepareAttackCheckOpponentBlockers(Guid userAttackerId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(userAttackerId)!;
            Room room = _roomManager.GetRoom(user)!;
            var blockers = _gameRuleService.GetBlockerCards(room.Opponent!, room, room.Game!);
            if (blockers.Count > 0)
            {
                var myGameInfo = room.Game!.GetMyPlayerInformation(userAttackerId);
                var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userAttackerId);
                var opponent = room.GetOpponent(userAttackerId);

                var flowAction = new FlowAction(user, opponent!, ResolveBlocker);
                var flowRequest = new FlowActionRequest(flowAction.Id, opponent!, "GAME_ASK_BLOCKER", blockers.Ids().ToList(), 1, 1, true);
                flowAction.Request = flowRequest;
                flowAction.FromCardId = attacker;
                flowAction.ToCardId = target;
                flowAction.FinalContext = FlowContext.ResolveAttack;

                return flowAction;
            }

            return null;
        }

        private RuleResponse ResolveBlocker(FlowArgs args)
        {
            var response = new RuleResponse();

            var room = _roomManager.GetRoom(args.User);
            var blockers = _gameRuleService.GetBlockerCards(room!.Opponent!, room, room.Game!);
            var blocker = blockers.FirstOrDefault(y => args.Response.CardsId.Any(x => x == y.Id));
            if (blocker != default)
            {
                response.FlowResponses.Add(new FlowResponseMessage("GAME_USE_BLOCKER", args.User.Username, blocker.CardInfo.Name));

                args.FlowAction.ToCardId = blocker.Id;
                var nextFlowAction = PrepareAttackCheckOpponentCounters(args.FlowAction.FromUser.Id, args.FlowAction.FromCardId!.Value, args.FlowAction.ToCardId!.Value);
                if (nextFlowAction != null)
                {
                    response.FlowAction = nextFlowAction;
                }

            }

            return response;
        }

        private FlowAction? PrepareAttackCheckOpponentCounters(Guid userAttackerId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(userAttackerId)!;
            Room room = _roomManager.GetRoom(user)!;
            var myGameInfo = room.Game!.GetMyPlayerInformation(userAttackerId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userAttackerId);
            var opponent = room.GetOpponent(userAttackerId);
            var counters = _gameRuleService.GetCounterCards(opponent!, room, room.Game);

            if (counters.Count > 0)
            {
                var flowAction = new FlowAction(user, opponent!, ResolveCounter);
                var flowRequest = new FlowActionRequest(flowAction.Id, opponent!, "GAME_ASK_COUNTER", counters.Ids().ToList(), 1, 99, true);
                flowAction.Request = flowRequest;
                flowAction.FromCardId = attacker;
                flowAction.ToCardId = target;
                flowAction.FinalContext = FlowContext.ResolveAttack;

                return flowAction;
            }

            return null;
        }

        private RuleResponse ResolveCounter(FlowArgs args)
        {
            return _gameRuleService.UseCounters(args.User, args.Room, args.Game, args.FlowAction.ToCardId!.Value, args.Response.CardsId);
        }

        private async Task<bool> ResolveAttack(Guid userId, Guid attacker, Guid defender)
        {
            try
            {
                //TODO Ne pas retourner une exception mais un Result erreur avec le message ?
                User user = _userManager.GetUser(userId)!;
                Room room = _roomManager.GetRoom(user)!;
                var result = _gameRuleService.Attack(user, room, room.Game!, attacker, defender);

                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);
                if (result.Success)
                {
                    await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_PLAYER_ATTACK_SUCCESS", result.AttackerGameInfo.Username, result.DefenderGameInfo.Username, result.AttackerCard.CardInfo.Name, result.DefenderCard.CardInfo.Name, result.AttackerPower.ToString(), result.DefenderPower.ToString()));
                    if (result.Winner)
                    {
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.GameFinished), userId);
                    }
                    else if (result.LifeCard != null)
                    {
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_PLAYER_LOOSE_LIFE", result.DefenderGameInfo.Username, result.DefenderGameInfo.Lifes.Count().ToString()));
                        var opponent = room.GetOpponent(user)!;
                        var opponentInfo = room.Game!.GetMyPlayerInformation(opponent.Id);
                        if (result.LifeCard.CardInfo.IsTrigger)
                        {
                            //TODO Ask for trigger life card or not
                            //If not, add to hand
                        }
                        else
                        {
                            await Clients.Group(opponent.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_GET_LIFE_CARD", result.LifeCard.CardInfo.Name));
                            await Clients.Group(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_GET_LIFE_CARD_ATTACKER", opponent.Username));
                            opponentInfo.AddHandCard(result.LifeCard);
                        }
                    }
                }
                else
                {
                    await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_PLAYER_ATTACK_FAILED", result.AttackerGameInfo.Username, result.DefenderGameInfo.Username, result.AttackerCard.CardInfo.Name, result.DefenderCard.CardInfo.Name, result.AttackerCard.GetTotalPower().ToString(), result.DefenderCard.GetTotalPower().ToString()));
                }
            } catch(Exception ex)
            {

            }

            return true;
        }
    }
}
