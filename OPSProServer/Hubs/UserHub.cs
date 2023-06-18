using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;

namespace OPSProServer.Hubs
{
    internal partial class GameHub : Hub, IUserHub
    {
        public Guid Register(string username)
        {
            try
            {
                var user = new User(Context.ConnectionId, username);
                _logger.LogInformation("Register new user {User}", user);

                _userManager.AddUser(user);

                return user.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return Guid.Empty;
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var user = _userManager.GetUser(Context.ConnectionId);
                _logger.LogInformation("User {User} disconnected", user);

                await LeaveRoom(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
