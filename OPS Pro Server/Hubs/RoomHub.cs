using Microsoft.AspNetCore.SignalR;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;
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
                    Description = description
                };

                _roomManager.AddRoom(room);
                user.CurrentRoom = room;

                await Groups.AddToGroupAsync(Context.ConnectionId, room.Id.ToString());

                return true;
            }

            return false;
        }

        public IReadOnlyList<Room> GetRooms()
        {
            return _roomManager.GetRooms();
        }

        public async Task<bool> JoinRoom(Guid id, Guid roomId, string? password)
        {
            var user = _userManager.GetUser(id);
            var room = _roomManager.GetRoom(roomId);
            if (user != null && room != null && room.IsJoinable(user, password))
            {
                room.Opponent = user;
                user.CurrentRoom = room;
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
                await Clients.Group(roomId.ToString()).SendAsync("room_full");
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
            if (user != null && user.CurrentRoom != null)
            {
                var room = user.CurrentRoom;

                user.CurrentRoom = null;
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Id.ToString());

                if (room.Creator.Id == user.Id)
                {
                    if (room.Opponent != null)
                    {
                        await Groups.RemoveFromGroupAsync(room.Opponent.ConnectionId, room.Id.ToString());
                        await Clients.Client(room.Opponent.ConnectionId).SendAsync("room_deleted");
                    }

                    _roomManager.RemoveRoom(room.Id);
                }
                else if (room.Opponent?.Id == user.Id)
                {
                    room.Opponent = null;
                    await Clients.Group(room.Id.ToString()).SendAsync("room_waiting");
                }
            }

            return true;
        }
    }
}
