using Microsoft.AspNetCore.SignalR;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;
using System.ComponentModel;

namespace OPSProServer.Hubs
{
    public partial class GameHub : Hub, IRoomHub
    {
        [Connected]
        public async Task<bool> CreateRoom(string? password, string? description)
        {
            var user = Context.Items["user"] as User; 
            var hasPassword = !string.IsNullOrEmpty(password);

            _logger.LogInformation("Creating room for user {Id} with password: {HasPassword} and description: {Description}", user!.Id, hasPassword, description);

            var room = new Room(user, description, password);

#if DEBUG
            room.SetOpponent(new User("serverbot", "Server Bot"));
            room.Opponent!.RPSChoice = RPSChoice.Paper;
#endif

            _roomManager.AddRoom(room);

            await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());

            _logger.LogInformation("Room created. Id: {Id}", room.Id);

            return true;
        }

        [Connected]
        public Task<List<SecureRoom>> GetRooms()
        {
            try
            {
                var rooms = _roomManager.GetRooms().Select(x => x as SecureRoom).ToList();
                return Task.FromResult(rooms);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Task.FromResult(new List<SecureRoom>());
            }
        }

        [Connected]
        public async Task<bool> JoinRoom(Guid roomId, string? password)
        {
            var user = Context.Items["user"] as User;

            _logger.LogInformation("User {Id} try to join room {RoomId}", user!.Id, roomId);
            var room = _roomManager.GetRoom(roomId);
            if (user != null && room != null && room.IsJoinable(user, password))
            {
                room.SetOpponent(user);
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);
                _logger.LogInformation("User {Id} joined room sucessfully", user.Id);

                return true;
            }

            return false;
        }

        [Connected(true)]
        public async Task<bool> LeaveRoom()
        {
            var user = Context.Items["user"] as User;
            return await LeaveRoom(user);
        }

        protected async Task<bool> LeaveRoom(User? user)
        {
            if (user != null)
            {
                var room = _roomManager.GetRoom(user);
                if (room != null)
                {
                    await Groups.RemoveFromGroupAsync(user.ConnectionId, room.Id.ToString());

                    if (room.Creator.Id == user.Id)
                    {
                        _logger.LogInformation("The leaver is the creator, disolving the room {Id}.", room.Id);
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomDeleted));

                        if (room.Opponent != null)
                        {
                            _logger.LogInformation("An opponent was in the room: {Id}, remove the user {Username} from the room.", room.Id, room.Opponent.Username);
                            await Groups.RemoveFromGroupAsync(room.Opponent.ConnectionId, room.Id.ToString());
                        }

                        _roomManager.RemoveRoom(room.Id);
                    }
                    else if (room.Opponent?.Id == user.Id)
                    {
                        _logger.LogInformation("The leaver is the opponent, remove the user from the room {Id}.", room.Id);
                        if (room.Game != null)
                        {
                            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomDeleted));
                            await Groups.RemoveFromGroupAsync(room.Creator.ConnectionId, room.Id.ToString());
                            _roomManager.RemoveRoom(room.Id);
                        } else
                        {
                            room.SetOpponent(null);
                            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);

                        }
                    }
                }
            }

            return true;
        }

        [Connected(true)]
        public async Task<bool> SetReady(string name, List<string> cardsId)//DeckInfo? deckInfo)
        {
            var user = Context.Items["user"] as User;
            var room = Context.Items["room"] as Room;

            DeckInfo deckInfo = new DeckInfo(name);
            foreach (var cardId in cardsId)
            {
                var cardInfo = _cardService.GetCardInfo(cardId);
                if (cardInfo != null)
                {
                    deckInfo.Cards.Add(cardInfo);
                }
            }

            if (deckInfo == null || !deckInfo.IsValid())
            {
                return false;
            }

            var userRoom = room!.GetUserRoom(user!);
            if (userRoom != null)
            {
                _logger.LogInformation("Update status for the creator {UserId} to {Ready}.", user.Id, true);
                userRoom.Deck = deckInfo;
            }

#if DEBUG
            room.Opponent!.Deck = deckInfo;
#endif

            await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);

            return true;
        }
    

        [Connected(true)]
        public async Task<bool> Exclude(Guid opponentId, Guid roomId)
        {
            try
            {
                var user = Context.Items["user"] as User;
                var room = Context.Items["room"] as Room;

                if (user != null && room != null && room.Creator.Id == user.Id && room.Opponent?.Id == opponentId)
                {
                    var opponent = room.GetOpponent(user.Id)!;
                    room.SetOpponent(null);
                    await Groups.RemoveFromGroupAsync(opponent.ConnectionId, room.Id.ToString());
                    await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);
                    await Clients.Client(opponent.ConnectionId).SendAsync(nameof(IRoomHubEvent.RoomExcluded));

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        [Connected(true)]
        public Task<SecureRoom> GetRoom()
        {
            var room = Context.Items["room"]!;
            return Task.FromResult((SecureRoom)room);
        }
    }
}
