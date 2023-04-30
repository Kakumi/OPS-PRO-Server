using Microsoft.AspNetCore.SignalR;
using OPS_Pro_Server.Managers;
using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;
using OPSProServer.Contracts.Hubs;

namespace OPS_Pro_Server.Hubs
{
    public partial class GameHub : Hub, IUserHub
    {
        public Guid Register(string username)
        {
            try
            {
                var guid = Guid.NewGuid();
                _logger.LogInformation($"Register new user {username} ({guid})");
                var user = new User(guid, Context.ConnectionId, username);

                _userManager.AddUser(user);

                return guid;
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
                _logger.LogInformation("User {Username} disconnected ({Id})", user?.Username, user?.Id);

                await LeaveRoom(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}
