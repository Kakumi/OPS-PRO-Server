using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Events;
using OPSProServer.Contracts.Exceptions;
using OPSProServer.Contracts.Models;
using OPSProServer.Managers;

namespace OPSProServer.Hubs.Filters
{
    public class ErrorHandlerFilter : IHubFilter
    {
        private readonly ILogger<GameHub> _logger;

        private readonly IUserManager _userManager;

        public ErrorHandlerFilter(ILogger<GameHub> logger, IUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (ErrorUserActionException ex)
            {
                var user = _userManager.GetUser(ex.UserId);
                if (user != null)
                {
                    await invocationContext.Hub.Clients.Client(user.ConnectionId).SendAsync(nameof(IGameHubEvent.UserAlertMessage), new UserAlertMessage(ex.Message, ex.Args));
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return false;
        }
    }
}
