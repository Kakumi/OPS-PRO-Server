using Microsoft.AspNetCore.SignalR;
using OPSProServer.Attributes;
using OPSProServer.Contracts.Exceptions;
using OPSProServer.Managers;
using System.Reflection;

namespace OPSProServer.Hubs.Filters
{
    public class UserConnectedHubFilter : IHubFilter
    {
        private readonly ILogger<GameHub> _logger;
        private readonly IUserManager _userManager;
        private readonly IRoomManager _roomManager;

        public UserConnectedHubFilter(ILogger<GameHub> logger, IUserManager userManager, IRoomManager roomManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roomManager = roomManager;
        }

        public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var connectedAttributes = invocationContext.HubMethod.GetCustomAttributes<ConnectedAttribute>();
            if (connectedAttributes != null && connectedAttributes.Count() > 0)
            {
                var attr = connectedAttributes.First();
                var user = _userManager.GetUser(invocationContext.Context.ConnectionId);
                if (user == null)
                {
                    return false;
                }

                invocationContext.Context.Items.Remove("user");
                invocationContext.Context.Items.Add("user", user);

                var room = _roomManager.GetRoom(user);

                if (attr.HasRoom && room == null)
                {
                    throw new ErrorUserActionException(user.Id, "ROOMS_NOT_CONNECTED");
                }

                if (!attr.HasRoom && room != null && attr.IsRoomMandatory)
                {
                    throw new ErrorUserActionException(user.Id, "ROOMS_ALREADY_HAS_ONE");
                }

                if (room != null)
                {
                    invocationContext.Context.Items.Remove("room");
                    invocationContext.Context.Items.Add("room", room);

                    if (attr.InGame)
                    {
                        if (room.Game == null)
                        {
                            throw new ErrorUserActionException(user.Id, "GAME_NOT_IN_GAME");
                        }

                        invocationContext.Context.Items.Remove("game");
                        invocationContext.Context.Items.Add("game", room.Game);

                        if (attr.IsTurn && user.Id != room.Game.PlayerTurn)
                        {
                            throw new ErrorUserActionException(user.Id, "GAME_NOT_YOUR_TURN");
                        }
                    }
                }

                return await next(invocationContext);
            }
            else
            {
                return await next(invocationContext);
            }

            return false;
        }
    }
}
