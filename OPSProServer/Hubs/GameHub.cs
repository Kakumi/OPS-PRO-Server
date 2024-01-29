using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Events;
using OPSProServer.Managers;
using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Exceptions;
using OPSProServer.Attributes;

namespace OPSProServer.Hubs
{
    public partial class GameHub : Hub, IGameHub
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

        [PlayerTurn]
        public async Task<bool> NextPhase(Guid userId)
        {
            var room = _roomManager.GetRoom(userId);
            room!.Game!.PhaseChanged += Game_PhaseChanged;
            await room.Game.UpdatePhase();
            room.Game.PhaseChanged -= Game_PhaseChanged;

            return true;
        }

        [PlayerTurn]
        public Task<bool> Attack(Guid userId, Guid attacker, Guid target)
        {
            throw new NotImplementedException();
        }

        [PlayerTurn]
        public async Task<bool> Summon(Guid userId, Guid cardId)
        {
            var room = _roomManager.GetRoom(userId);
            var gameInfo = room!.Game!.GetCurrentPlayerGameInformation();
            gameInfo.Summon(cardId);
            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IGameHubEvent.BoardUpdated), room.Game);

            return true;
        }
    }
}
