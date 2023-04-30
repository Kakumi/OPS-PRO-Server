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
            var user = _userManager.GetUser(id);
            if (user != null)
            {
                var room = new Room(user)
                {
                    Password = password,
                    UsePassword = !string.IsNullOrWhiteSpace(password),
                    Description = description
                };

                _roomManager.AddRoom(room);

                await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());

                return true;
            }

            return false;
        }

        public List<Room> GetRooms()
        {
            //Remove password from room
            var rooms = _roomManager.GetRooms().Select(x => x.Clone()).ToList();
            rooms.ForEach(x => x.Password = null);
            return rooms;
        }
        
        public async Task<bool> JoinRoom(Guid id, Guid roomId, string? password)
        {
            var user = _userManager.GetUser(id);
            var room = _roomManager.GetRoom(roomId);
            if (user != null && room != null && room.IsJoinable(user, password))
            {
                room.Opponent = user;
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);
                return true;
            }

            return false;
        }

        public async Task<bool> LeaveRoom(Guid id)
        {
            var user = _userManager.GetUser(id);
            return await LeaveRoom(user);
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
                        await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomDeleted));

                        if (room.Opponent != null)
                        {
                            await Groups.RemoveFromGroupAsync(room.Opponent.ConnectionId, room.Id.ToString());
                        }

                        _roomManager.RemoveRoom(room.Id);
                    }
                    else if (room.Opponent?.Id == user.Id)
                    {
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
            var user = _userManager.GetUser(userId);
            if (user != null)
            {
                var room = _roomManager.GetRoom(user);
                if (room != null)
                {
                    if (room.Creator == user)
                    {
                        room.CreatorReady = ready;
                    }
                    else
                    {
                        room.OpponentReady = ready;
                    }

                    await Clients.Group(room.Id.ToString()).SendAsync(nameof(IRoomHubEvent.RoomUpdated), room);

                    return ready;
                }
            }

            return false;
        }
    }
}
