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
            var blockers = room.Game!.GetBlockerCards(room.Opponent!);
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

            var blockers = args.Room!.Game!.GetBlockerCards(args.Room!.Opponent!);
            var blocker = blockers.FirstOrDefault(y => args.Response.CardsId.Any(x => x == y.Id));
            if (blocker != default)
            {
                response.FlowResponses.Add(new FlowResponseMessage("GAME_USE_BLOCKER", args.User.Username, blocker.CardInfo.Name));
                response.Add(args.Game.OnUseBlocker(args.User, blocker));

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
            var counters = room.Game.GetCounterCards(opponent!);

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
            return args.Room.Game!.UseCounters(args.User, args.FlowAction.ToCardId!.Value, args.Response.CardsId);
        }

        private async Task<FlowAction?> ResolveAttack(Guid userId, Guid attacker, Guid defender)
        {
            var response = new RuleResponse();
            //TODO Ne pas retourner une exception mais un Result erreur avec le message ?
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var result = room.Game!.Attack(user, room.GetOpponent(userId)!, attacker, defender);
            response.Add(result);

            foreach(var message in response.FlowResponses)
            {
                await SendFlowMessage(room, message);
            }

            if (response.Winner != null)
            {
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.GameFinished), userId);
                return null;
            }

            return response.FlowAction;
        }
    }
}
