using Microsoft.AspNetCore.SignalR;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Exceptions;
using OPSProServer.Contracts.Models;
using OPSProServer.Managers;
using System.Reflection;

namespace OPSProServer.Hubs.Filters
{
    public class PlayerTurnFilter : IHubFilter
    {
        private readonly ILogger<GameHub> _logger;

        private readonly IUserManager _userManager;
        private readonly IRoomManager _roomManager;

        public PlayerTurnFilter(ILogger<GameHub> logger, IUserManager userManager, IRoomManager roomManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roomManager = roomManager;
        }

        public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var attrs = invocationContext.HubMethod.GetCustomAttributes<PlayerTurnAttribute>();
            if (attrs != null && attrs.Count() > 0)
            {
                var user = _userManager.GetUser(invocationContext.Context.ConnectionId);
                if (user != null)
                {
                    var room = _roomManager.GetRoom(user);
                    if (room != null && room.Opponent != null && room.Game != null)
                    {
                        if (room.Game.PlayerTurn == user.Id)
                        {
                            return await next(invocationContext);
                        }
                    }
                }
            } else
            {
                return await next(invocationContext);
            }

            return null;
        }
    }
}
