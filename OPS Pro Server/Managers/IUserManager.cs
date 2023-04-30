using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;

namespace OPS_Pro_Server.Managers
{
    public interface IUserManager
    {
        User? GetUser(Guid id);
        User? GetUser(string connectionId);
        void AddUser(User user);
        void RemoveUser(User user);
    }
}
