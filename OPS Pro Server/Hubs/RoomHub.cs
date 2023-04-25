using Microsoft.AspNetCore.SignalR;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;

namespace OPS_Pro_Server.Hubs
{
    public class RoomHub : Hub
    {
        private readonly ILogger<RoomHub> _logger;
        private readonly IRoomManager _roomManager;
        private readonly IUserManager _userManager;

        public RoomHub(ILogger<RoomHub> logger, IRoomManager roomManager, IUserManager userManager)
        {
            _logger = logger;
            _roomManager = roomManager;
            _userManager = userManager;
        }

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

        public async Task JoinRoom(Guid id)
        {
            var user = _userManager.GetUser(id);
            await Clients.All.SendAsync("test", user);
        }
    }
}
