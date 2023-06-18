using OPSProServer.Contracts.Models;

namespace OPSProServer.Managers
{
    public interface IUserManager
    {
        User? GetUser(Guid id);
        User? GetUser(string connectionId);
        void AddUser(User user);
        void RemoveUser(User user);
    }
}
