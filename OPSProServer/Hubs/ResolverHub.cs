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

        //[Connected(true, true, true)]
        //public async Task<bool> ResolveAction(Guid userId, Guid userResolverId, List<Guid> cards)
        //{
        //    var user = Context.Items["user"] as User;
        //    var room = Context.Items["room"] as Room;
        //    bool errorWithResolver = true;
        //    var userResolver = _resolverManager.GetUserResolver(userResolverId);
        //    if (userResolver != null)
        //    {
        //        var resolver = _resolverManager.GetResolver(userResolver.ResolverId);
        //        if (resolver != null)
        //        {
        //            errorWithResolver = false;

        //            if (userResolver.Type == ActionResolverType.Blocker && resolver.FromCardId != null && resolver.ToCardId != null)
        //            {
        //                return await ResolveBlocker(userResolver, resolver, user, room, cards);
        //            }

        //            if (userResolver.Type == ActionResolverType.Counter && resolver.FromCardId != null && resolver.ToCardId != null)
        //            {
        //                return await ResolveCounter(userResolver, resolver, user, room, cards);
        //            }
        //        }
        //    }

        //    if (errorWithResolver)
        //    {
        //        room.Game!.CreatorGameInformation.Waiting = false;
        //        room.Game!.OpponentGameInformation.Waiting = false;
        //        await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserAlertMessage), new UserAlertMessage("GAME_RESOLVER_FAILED"));
        //        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.WaitOpponent), false);
        //    }

        //    return true;
        //}

        //[Connected(true, true, true)]
        //public Task<bool> ResolveAskAction(Guid userId, Guid actionId, bool value)
        //{
        //    throw new NotImplementedException();
        //}

        //private async Task<bool> ResolveBlocker(UserResolver userResolver, ActionResolver resolver, User user, Room room, List<Guid> cards)
        //{
        //    var blockers = _gameRuleService.GetBlockerCards(room.Opponent!, room.Game!);
        //    var cardId = cards.FirstOrDefault(x => blockers.Any(y => x == y.Id));
        //    if (cardId != default)
        //    {
        //        resolver.ToCardId = cardId;
        //        var sent = await CheckCounters(user.Id, resolver.FromCardId!.Value, resolver.ToCardId.Value, resolver.Id);
        //        if (sent)
        //        {
        //            return true;
        //        }

        //        return await ResolveAttack(user.Id, resolver.FromCardId.Value, resolver.ToCardId.Value);
        //    }

        //    return false;
        //}

        //private async Task<bool> ResolveCounter(UserResolver userResolver, ActionResolver resolver, User user, Room room, List<Guid> cards)
        //{
        //    var response = _gameRuleService.UseCounters(user, room.Game!, resolver.FromCardId!.Value, cards);
        //    foreach (var message in response.CodesMessage)
        //    {
        //        await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), message);
        //    }

        //    return await ResolveAttack(user.Id, resolver.FromCardId.Value, resolver.ToCardId!.Value);
        //}

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

                //var resolver = new ActionResolver(CardAction.Attack, attacker, target, userId);
                //_resolverManager.AddResolver(resolver);
                //var userResolver = new UserResolver(resolver.Id, ActionResolverType.Blocker, "GAME_ASK_BLOCKER", blockers.Ids().ToList(), 1, 1, true);
                //_resolverManager.AddUserResolver(userResolver);
                //await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.AskUserAction), userResolver);

                //myGameInfo.Waiting = true;
                //await Clients.Client(user!.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), true);
                return flowAction;
            }

            return null;
        }

        private List<FlowResponseMessage> ResolveBlocker(FlowArgs args)
        {
            var list = new List<FlowResponseMessage>();

            var room = _roomManager.GetRoom(args.User);
            var blockers = _gameRuleService.GetBlockerCards(room!.Opponent!, room, room.Game!);
            var blocker = blockers.FirstOrDefault(y => args.Response.CardsId.Any(x => x == y.Id));
            if (blocker != default)
            {
                list.Add(new FlowResponseMessage(room, new UserGameMessage("GAME_USE_BLOCKER", args.User.Username, blocker.CardInfo.Name)));

                args.FlowAction.ToCardId = blocker.Id;
                var nextFlowAction = PrepareAttackCheckOpponentCounters(args.FlowAction.FromUser.Id, args.FlowAction.FromCardId!.Value, args.FlowAction.ToCardId!.Value);
                //var sent = await CheckCounters(user.Id, resolver.FromCardId!.Value, resolver.ToCardId.Value, resolver.Id);
                //if (sent)
                //{
                //    return true;
                //}
                if (nextFlowAction != null)
                {
                    args.FlowAction.AddLast(nextFlowAction);
                }
                else
                {
                    _ = ResolveAttack(args.FlowAction.FromUser.Id, args.FlowAction.FromCardId!.Value, args.FlowAction.ToCardId!.Value);
                    //await ResolveAttack(user.Id, resolver.FromCardId.Value, resolver.ToCardId.Value);
                }

            }

            return list;
        }

        private FlowAction? PrepareAttackCheckOpponentCounters(Guid userAttackerId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(userAttackerId)!;
            Room room = _roomManager.GetRoom(user)!;
            var myGameInfo = room.Game!.GetMyPlayerInformation(userAttackerId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userAttackerId);
            //TODO TESTING 
            var opponent = room.GetOpponent(userAttackerId);
            var counters = _gameRuleService.GetCounterCards(opponent!, room, room.Game);

            if (counters.Count > 0)
            {
                var flowAction = new FlowAction(user, opponent!, ResolveCounter);
                var flowRequest = new FlowActionRequest(flowAction.Id, opponent!, "GAME_ASK_COUNTER", counters.Ids().ToList(), 1, 99, true);
                flowAction.Request = flowRequest;
                flowAction.FromCardId = attacker;
                flowAction.ToCardId = target;

                return flowAction;

                //var userResolver = new UserResolver(resolver.Id, ActionResolverType.Counter, "GAME_ASK_COUNTER", counters.Select(x => x.Id).ToList(), 1, 99, true);
                //_resolverManager.AddUserResolver(userResolver);
                //await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.AskUserAction), userResolver);

                //myGameInfo.Waiting = true;
                //await Clients.Client(user!.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), true);
                //return true;
            }

            return null;
        }

        private List<FlowResponseMessage> ResolveCounter(FlowArgs args)
        {
            var list = new List<FlowResponseMessage>();

            var response = _gameRuleService.UseCounters(args.User, args.Room, args.Game, args.FlowAction.ToCardId!.Value, args.Response.CardsId);
            list.AddRange(response.FlowResponses);

            _ = ResolveAttack(args.FlowAction.FromUser.Id, args.FlowAction.FromCardId!.Value, args.FlowAction.ToCardId!.Value);

            return list;
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
                    await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_PLAYER_ATTACK_SUCCESS", result.AttackerGameInfo.Username, result.DefenderGameInfo.Username, result.AttackerCard.CardInfo.Name, result.DefenderCard.CardInfo.Name, result.AttackerCard.GetTotalPower().ToString(), result.DefenderCard.GetTotalPower().ToString()));
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
