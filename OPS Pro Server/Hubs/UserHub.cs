using Microsoft.AspNetCore.SignalR;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;

namespace OPS_Pro_Server.Hubs
{
    public class UserHub : Hub
    {
        private readonly ILogger<UserHub> _logger;
        private readonly IRoomManager _roomManager;
        private readonly IUserManager _userManager;

        public UserHub(ILogger<UserHub> logger, IRoomManager roomManager, IUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roomManager = roomManager;
        }

        public Guid Register(string username)
        {
            var guid = Guid.NewGuid();
            _logger.LogInformation($"Register new user {username} ({guid})");
            var user = new User(guid, Context.ConnectionId, username);

            _userManager.AddUser(user);

            return guid;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var user = _userManager.GetUser(Context.ConnectionId);
            if (user != null)
            {
                _logger.LogInformation($"User {user.UserName} disconnected ({user.Id}); {exception?.Message}");
                _userManager.RemoveUser(user);

                if (user.CurrentRoom != null && user.CurrentRoom.Opponent != null)
                {
                    await Clients.Group(user.CurrentRoom.Id.ToString()).SendAsync("room_deleted");
                    //TODO Delete users from group
                    //user.CurrentRoom.Opponent.CurrentRoom = null;
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
