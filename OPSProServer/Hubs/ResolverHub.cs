using Microsoft.AspNetCore.SignalR;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Metrics;

namespace OPSProServer.Hubs
{
    public partial class GameHub : Hub, IResolverHub
    {
        private FlowAction PrepareAttackCheckOpponentBlockers(Guid userAttackerId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(userAttackerId)!;
            Room room = _roomManager.GetRoom(user)!;

            var myGameInfo = room.Game!.GetMyPlayerInformation(userAttackerId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userAttackerId);
            var opponent = room.GetOpponent(userAttackerId);

            var flowAction = new FlowAction(user, opponent!, ResolveBlocker, CanResolveBlocker);
            flowAction.FromCardId = attacker;
            flowAction.ToCardId = target;
            flowAction.FinalContext = FlowContext.ResolveAttack;

            return flowAction;
        }

        private bool CanResolveBlocker(User user, Room room, Game game, FlowAction action)
        {
            var blockers = room.Game!.GetBlockerCards(action.ToUser);
            if (blockers.Count > 0)
            {
                var gameInfo = game.GetMyPlayerInformation(action.FromUser.Id);
                var opponentGameInfo = game.GetMyPlayerInformation(action.ToUser.Id);
                var attacker = gameInfo.GetCard(action.FromCardId!.Value);
                var defender = opponentGameInfo.GetCard(action.ToCardId!.Value);

                if (attacker != null && defender != null)
                {
                    action.Request = new FlowActionRequest(action.Id, action.ToUser, "GAME_ASK_BLOCKER", blockers.Ids().ToList(), 1, 1, true, attacker.CardInfo.Name, attacker.GetTotalPower().ToString(), defender.CardInfo.Name, defender.GetTotalPower().ToString());
                }

                return true;
            }

            return false;
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
            }

            return response;
        }

        private FlowAction PrepareAttackCheckOpponentCounters(Guid userAttackerId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(userAttackerId)!;
            Room room = _roomManager.GetRoom(user)!;
            var myGameInfo = room.Game!.GetMyPlayerInformation(userAttackerId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userAttackerId);
            var opponent = room.GetOpponent(userAttackerId);

            var flowAction = new FlowAction(user, opponent!, ResolveCounter, CanResolveCounter);
            flowAction.FromCardId = attacker;
            flowAction.ToCardId = target;
            flowAction.FinalContext = FlowContext.ResolveAttack;

            return flowAction;
        }

        private bool CanResolveCounter(User user, Room room, Game game, FlowAction action)
        {
            var counters = game.GetCounterCards(action.ToUser);
            if (counters.Count > 0)
            {
                var gameInfo = game.GetMyPlayerInformation(action.FromUser.Id);
                var opponentGameInfo = game.GetMyPlayerInformation(action.ToUser.Id);
                var attacker = gameInfo.GetCard(action.FromCardId!.Value);
                var defender = opponentGameInfo.GetCard(action.ToCardId!.Value);

                if (attacker != null && defender != null)
                {
                    action.Request = new FlowActionRequest(action.Id, action.ToUser, "GAME_ASK_COUNTER", counters.Ids().ToList(), 1, 99, true, attacker.CardInfo.Name, attacker.GetTotalPower().ToString(), defender.CardInfo.Name, defender.GetTotalPower().ToString());
                }

                return true;
            }

            return false;
        }

        private RuleResponse ResolveCounter(FlowArgs args)
        {
            return args.Room.Game!.UseCounters(args.User, args.FlowAction.ToCardId!.Value, args.Response.CardsId);
        }

        private async Task<RuleResponse?> ResolveAttack(Guid userId, Guid attacker, Guid defender)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var response = room.Game!.Attack(user, room.GetOpponent(userId)!, attacker, defender);

            if (response.Winner != null)
            {
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.GameFinished), userId);
                return null;
            }

            return response;
        }

        private FlowAction PrepareAttackEventCounter(Guid callerId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(callerId)!;
            Room room = _roomManager.GetRoom(user)!;
            var myGameInfo = room.Game!.GetMyPlayerInformation(callerId);
            var opponentGameInfo = room.Game!.GetMyPlayerInformation(room.Opponent!.Id);
            var opponent = opponentGameInfo.User;

            var flowAction = new FlowAction(user, opponent!, ResolveEventCounter, CanResolveEventCounter);
            flowAction.FromCardId = attacker;
            flowAction.ToCardId = target;
            flowAction.FinalContext = FlowContext.ResolveAttack;

            return flowAction;
        }

        private bool CanResolveEventCounter(User user, Room room, Game game, FlowAction action)
        {
            var eventCounters = game.GetEventCounterCards(action.ToUser);
            var gameInfo = game.GetMyPlayerInformation(action.ToUser.Id);
            var availableEventCounters = eventCounters.Where(x => x.GetTotalCost() <= gameInfo.DonAvailable).ToList();
            if (availableEventCounters.Count > 0)
            {
                action.Request = new FlowActionRequest(action.Id, action.ToUser, "GAME_ASK_EVENT_COUNTER_ATTACK", availableEventCounters.Ids().ToList(), 0, 99, true, action.FromUser.Username);

                return true;
            }

            return false;
        }

        private RuleResponse ResolveEventCounter(FlowArgs args)
        {
            return args.Room.Game!.UseEventCounters(args.User, args.Response.CardsId);
        }
    }
}
