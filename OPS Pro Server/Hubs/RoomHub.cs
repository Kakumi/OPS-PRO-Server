using Microsoft.AspNetCore.SignalR;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Hubs;

namespace OPS_Pro_Server.Hubs
{
    public partial class GameHub : Hub, IRoomHub
    {
        public async Task<bool> CreateRoom(Guid id, string? password, string? description)
        {
            try
            {
                var hasPassword = !string.IsNullOrEmpty(password);
                _logger.LogInformation("Creating room for user {Id} with password: {HasPassword} and description: {Description}", id, hasPassword, description);
                var user = _userManager.GetUser(id);
                if (user != null)
                {
                    var room = new Room(user)
                    {
                        Password = hasPassword ? password : null,
                        UsePassword = hasPassword,
                        Description = description
                    };

                    _roomManager.AddRoom(room);

                    await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());

                    _logger.LogInformation("Room created. Id: {Id}", room.Id);

                    return true;
                }

                return false;
            } catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public List<Room> GetRooms()
        {
            try
            {
                //Remove password from room
                var rooms = _roomManager.GetRooms().Select(x => x.Clone()).ToList();
                rooms.ForEach(x => x.Password = null);
                return rooms;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new List<Room>();
            }
        }
        
        public async Task<bool> JoinRoom(Guid id, Guid roomId, string? password)
        {
            try
            {
                _logger.LogInformation("User {Id} try to join room {RoomId}", id, roomId);
                var user = _userManager.GetUser(id);
                var room = _roomManager.GetRoom(roomId);
                if (user != null && room != null && room.IsJoinable(user, password))
                {
                    room.Opponent = user;
                    await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                    await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);
                    _logger.LogInformation("User {Id} joined room sucessfully", id);

                    await Exclude(Guid.NewGuid(), id, roomId);

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

        public async Task<bool> LeaveRoom(Guid id)
        {
            try
            {
                _logger.LogInformation("User: {Id} try to leave his current room.", id);
                var user = _userManager.GetUser(id);
                return await LeaveRoom(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        protected async Task<bool> LeaveRoom(User? user)
        {
            if (user != null)
            {
                var room = _roomManager.GetRoom(user);
                if (room != null)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Id.ToString());

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
                        room.Opponent = null;
                        room.OpponentReady = false;
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);
                    }
                }
            }

            return true;
        }

        public async Task<bool> SetReady(Guid userId, bool ready)
        {
            try
            {
                var user = _userManager.GetUser(userId);
                if (user != null)
                {
                    var room = _roomManager.GetRoom(user);
                    if (room != null)
                    {
                        if (room.Creator == user)
                        {
                            _logger.LogInformation("Update status for the creator {UserId} to {Ready}.", userId, ready);
                            room.CreatorReady = ready;
                        }
                        else
                        {
                            _logger.LogInformation("Update status for the opponent {UserId} to {Ready}.", userId, ready);
                            room.OpponentReady = ready;
                        }

                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);

                        return ready;
                    }
                    else
                    {
                        _logger.LogInformation("Failed to update ready status for the user {UserId}, room doesn't exist.", userId);
                    }
                }
                else
                {
                    _logger.LogInformation("Failed to update ready status for the user {UserId}, user doesn't exist.", userId);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public async Task<bool> Exclude(Guid userId, Guid opponentId, Guid roomId)
        {
            try
            {
                var user = _userManager.GetUser(userId);
                var opponent = _userManager.GetUser(opponentId);
                var room = _roomManager.GetRoom(roomId);

                if (user != null && opponent != null && room != null && room.Creator.Id == userId && room.Opponent?.Id == opponentId)
                {
                    room.Opponent = null;
                    room.OpponentReady = false;

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
    }
}
