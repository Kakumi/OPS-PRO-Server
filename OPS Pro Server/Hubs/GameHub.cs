using Microsoft.AspNetCore.SignalR;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;

namespace OPS_Pro_Server.Hubs
{
    public partial class GameHub : Hub
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
    }
}
