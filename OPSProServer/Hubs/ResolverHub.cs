using Microsoft.AspNetCore.SignalR;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;

namespace OPSProServer.Hubs
{
    public partial class GameHub : Hub, IResolverHub
    {

        [InGame]
        public async Task<bool> ResolveAction(Guid userId, Guid userResolverId, List<Guid> cards)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            bool errorWithResolver = true;
            var userResolver = _resolverManager.GetUserResolver(userResolverId);
            if (userResolver != null)
            {
                var resolver = _resolverManager.GetResolver(userResolver.ResolverId);
                if (resolver != null)
                {
                    errorWithResolver = false;

                    if (userResolver.Type == ActionResolverType.Blocker && resolver.FromCardId != null && resolver.ToCardId != null)
                    {
                        return await ResolveBlocker(userResolver, resolver, user, room, cards);
                    }

                    if (userResolver.Type == ActionResolverType.Counter && resolver.FromCardId != null && resolver.ToCardId != null)
                    {
                        return await ResolveCounter(userResolver, resolver, user, room, cards);
                    }
                }
            }

            if (errorWithResolver)
            {
                room.Game!.CreatorGameInformation.Waiting = false;
                room.Game!.OpponentGameInformation.Waiting = false;
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserAlertMessage), new UserAlertMessage("GAME_RESOLVER_FAILED"));
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.WaitOpponent), false);
            }

            return true;
        }

        [InGame]
        public Task<bool> ResolveAskAction(Guid userId, Guid actionId, bool value)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> ResolveBlocker(UserResolver userResolver, ActionResolver resolver, User user, Room room, List<Guid> cards)
        {
            var blockers = _gameRuleService.GetBlockerCards(room.Opponent!, room.Game!);
            var cardId = cards.FirstOrDefault(x => blockers.Any(y => x == y.Id));
            if (cardId != default)
            {
                resolver.ToCardId = cardId;
                var sent = await CheckCounters(user.Id, resolver.FromCardId!.Value, resolver.ToCardId.Value, resolver.Id);
                if (sent)
                {
                    return true;
                }

                return await ResolveAttack(user.Id, resolver.FromCardId.Value, resolver.ToCardId.Value);
            }

            return false;
        }

        private async Task<bool> ResolveCounter(UserResolver userResolver, ActionResolver resolver, User user, Room room, List<Guid> cards)
        {
            var response = _gameRuleService.UseCounters(user, room.Game!, resolver.FromCardId!.Value, cards);
            foreach (var message in response.CodesMessage)
            {
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserGameMessage), message);
            }

            return await ResolveAttack(user.Id, resolver.FromCardId.Value, resolver.ToCardId!.Value);
        }

        private async Task<bool> CheckBlockers(Guid userId, Guid attacker, Guid target)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var blockers = _gameRuleService.GetBlockerCards(room.Opponent!, room.Game);
            if (blockers.Count > 0)
            {
                var myGameInfo = room.Game!.GetMyPlayerInformation(userId);
                var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userId);
                var opponent = room.GetOpponent(userId);

                var resolver = new ActionResolver(CardAction.Attack, attacker, target, userId);
                _resolverManager.AddResolver(resolver);
                var userResolver = new UserResolver(resolver.Id, ActionResolverType.Blocker, "GAME_ASK_BLOCKER", blockers.Ids().ToList(), 1, 1, true);
                _resolverManager.AddUserResolver(userResolver);
                await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.AskUserAction), userResolver);

                myGameInfo.Waiting = true;
                await Clients.Client(user!.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), true);
                return true;
            }

            return false;
        }

        private async Task<bool> CheckCounters(Guid userId, Guid attacker, Guid target, Guid? resolverId)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var myGameInfo = room.Game!.GetMyPlayerInformation(userId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userId);
            var opponent = room.GetOpponent(userId);//TODO TESTING user;
            var counters = _gameRuleService.GetCounterCards(opponent, room.Game);

            if (counters.Count > 0)
            {
                ActionResolver? resolver = _resolverManager.GetResolver(resolverId);
                if (resolver == null)
                {
                    resolver = new ActionResolver(CardAction.Attack, attacker, target, userId);
                    _resolverManager.AddResolver(resolver);
                }

                var userResolver = new UserResolver(resolver.Id, ActionResolverType.Counter, "GAME_ASK_COUNTER", counters.Select(x => x.Id).ToList(), 1, 99, true);
                _resolverManager.AddUserResolver(userResolver);
                await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.AskUserAction), userResolver);

                myGameInfo.Waiting = true;
                await Clients.Client(user!.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), true);
                return true;
            }

            return false;
        }

        private async Task<bool> ResolveAttack(Guid userId, Guid attacker, Guid defender)
        {
            User user = _userManager.GetUser(userId)!;
            Room room = _roomManager.GetRoom(user)!;
            var result = _gameRuleService.Attack(user, room.Game!, attacker, defender);

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
                    //TODO Ask for trigger life card or not
                    //If not, add to hand
                }
            }
            else
            {
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_PLAYER_ATTACK_FAILED", result.AttackerGameInfo.Username, result.DefenderGameInfo.Username, result.AttackerCard.CardInfo.Name, result.DefenderCard.CardInfo.Name, result.AttackerCard.GetTotalPower().ToString(), result.DefenderCard.GetTotalPower().ToString()));
            }

            return true;
        }
    }
}
