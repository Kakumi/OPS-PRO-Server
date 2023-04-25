using OPS_Pro_Server.Models;

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
