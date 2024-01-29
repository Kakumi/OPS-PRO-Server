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

        public GameHub(ILogger<GameHub> logger, ICardService cardService, IRoomManager roomManager, IUserManager userManager, IResolverManager resolverManager)
        {
            _logger = logger;
            _cardService = cardService;
            _roomManager = roomManager;
            _userManager = userManager;
            _resolverManager = resolverManager;
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
                        //Idée:
                        //Pour la gestion des events de cartes:
                        //1. On va regarder les cartes des joueurs et voir si sur base de l'action
                        //on peut appliquer quelque chose
                        //2. Si oui, on va créer une "requête" avec un ID et l'envoyer au joueur
                        //3. Le joueur réagit ou pas
                        //  Si sa impliquer un autre joueur alors on va créer une autre requête avec un autre ID pour connaître la hiérarchie
                        //  De cette manière à force on va forcément devoir revenir en arrière et revenir à la requête d'origine.
                        //  Pour matérialisé ça, on peut avoir un "stack" dans la game qui contient la liste des requêtes
                        //  entre les deux joueurs, quand la liste est vide alors on peut continuer le fonctionnement et mettre
                        //  a jour le board (peut-être avoir un event quand chaque requête est crée / finie)
                        //4. Le processus continue et on change de phase / action
                        //Pour faire ça il faudrait une méthode :
                        //  async Task CheckUserInputs qui notifie un joueur qu'il peut réagir et choisir avec quoi il veut réagir
                        //Type d'action:
                        //  - Attaque
                        //  - Fin de tour
                        //  - Début de tour
                        //  - Début de manche X
                        //  - Fin de manche X
                        //  - Draw

                        //Comme la gestion des cartes ne sera pas asynchrone il faudra renvoyer un type d'action à faire depuis le serveur
                        //Du genre, demander à l'adversaire de supprimer une carte

                        if (e.NewPhaseType == PhaseType.Opponent && e.OldPhaseType == PhaseType.End)
                        {
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
            var room = _roomManager.GetRoom(userId);
            room!.Game!.PhaseChanged += Game_PhaseChanged;
            await room.Game.UpdatePhase();
            room.Game.PhaseChanged -= Game_PhaseChanged;

            return true;
        }

        [InGame]
        public async Task<bool> Attack(Guid userId, Guid attacker, Guid target)
        {
            Room room = _roomManager.GetRoom(userId)!;
            var user = _userManager.GetUser(userId);
            var myGameInfo = room.Game!.GetMyPlayerInformation(userId);
            var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userId);
            var attackerCard = myGameInfo.GetCharacter(attacker);
            var defenderCard = opponentGameInfo.GetCharacter(target);
            if (attackerCard != null && defenderCard != null)
            {
                //Check blocker
                if (false)
                {
                    var opponent = room.GetOpponent(userId);
                    var resolver = new ActionResolver(CardAction.Attack, attacker, target, userId);
                    _resolverManager.AddResolver(resolver);
                    var userResolver = new UserResolver(resolver.Id, ActionResolverType.Blocker, "GAME_ASK_BLOCKER", new List<Guid>());
                    _resolverManager.AddUserResolver(userResolver);
                    await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.AskUserAction), userResolver);
                    await Clients.Client(user!.ConnectionId).SendAsync(nameof(IGameHubEvent.WaitOpponent), true);
                    return true;
                }

                //Check counter
                var sent = await CheckCounters(userId, attacker, target, null);
                if (sent)
                {
                    return true;
                }

                return await ResolveAttack(userId, attacker, target);
            }

            throw new ErrorUserActionException(userId, "GAME_CARD_NOT_FOUND");
        }

        private async Task<bool> CheckCounters(Guid userId, Guid attacker, Guid target, Guid? resolverId)
        {
            if (false)
            {
                Room room = _roomManager.GetRoom(userId)!;
                var opponentGameInfo = room.Game!.GetOpponentPlayerInformation(userId);
                var opponent = room.GetOpponent(userId);
                ActionResolver? resolver = _resolverManager.GetResolver(resolverId);
                if (resolver == null)
                {
                    resolver = new ActionResolver(CardAction.Attack, attacker, target, userId);
                }

                var userResolver = new UserResolver(resolver.Id, ActionResolverType.Counter, "GAME_ASK_COUNTER", new List<Guid>());
                _resolverManager.AddUserResolver(userResolver);
                await Clients.Client(opponent!.ConnectionId).SendAsync(nameof(IGameHubEvent.AskUserAction), userResolver);
                return true;
            }

            return false;
        }

        private async Task<bool> ResolveAttack(Guid userId, Guid attacker, Guid defender)
        {
            Room room = _roomManager.GetRoom(userId)!;
            var result = room.Game!.Attack(userId, attacker, defender);

            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);
            if (result.Success)
            {
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_PLAYER_ATTACK_SUCCESS", result.AttackerGameInfo.Username, result.DefenderGameInfo.Username, result.AttackerCard.CardInfo.Name, result.DefenderCard.CardInfo.Name));
            }
            else
            {
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.UserGameMessage), new UserGameMessage("GAME_PLAYER_ATTACK_FAILED", result.AttackerGameInfo.Username, result.DefenderGameInfo.Username, result.AttackerCard.CardInfo.Name, result.DefenderCard.CardInfo.Name));
            }

            return true;
        }

        [InGame]
        public Task<bool> GiveDonCard(Guid userId, Guid characterCardId)
        {
            throw new NotImplementedException();
        }

        [InGame]
        public async Task<bool> Summon(Guid userId, Guid cardId)
        {
            var room = _roomManager.GetRoom(userId);
            var gameInfo = room!.Game!.GetCurrentPlayerGameInformation();
            gameInfo.Summon(cardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);

            return true;
        }

        [InGame]
        public Task<bool> ActivateCardEffect(Guid userId, Guid characterCardId)
        {
            throw new NotImplementedException();
        }

        [InGame]
        public async Task<bool> ResolveAction(Guid userId, Guid userResolverId, List<Guid> cards)
        {
            Room room = _roomManager.GetRoom(userId)!;
            User user = _userManager.GetUser(userId)!;
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
                        var cardId = cards.FirstOrDefault(x => true);
                        if (cardId != default)
                        {
                            resolver.ToCardId = cardId;
                            var sent = await CheckCounters(userId, resolver.FromCardId.Value, resolver.ToCardId.Value, resolver.Id);
                            if (sent)
                            {
                                return true;
                            }

                            return await ResolveAttack(userId, resolver.FromCardId.Value, resolver.ToCardId.Value);

                        }
                    }
                }
            }

            if (errorWithResolver)
            {
                await Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserAlertMessage), new UserAlertMessage("GAME_RESOLVER_FAILED"));
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.WaitOpponent), false);
            }

            return true;
        }
    }
}
