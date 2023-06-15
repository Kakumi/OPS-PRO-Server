using OPSProServer.Models;

namespace OPSProServer.Managers
{
    public class UserManager : IUserManager
    {
        private readonly List<User> _users;

        public UserManager()
        {
            _users = new List<User>();
        }

        public User? GetUser(Guid id)
        {
            return _users.FirstOrDefault(x => x.Id.Equals(id));
        }

        public User? GetUser(string connectionId)
        {
            return _users.FirstOrDefault(x => x.ConnectionId.Equals(connectionId));
        }

        public void AddUser(User user)
        {
            if (!_users.Any(x => x.Id == user.Id))
            {
                _users.Add(user);
            }
        }

        public void RemoveUser(User user)
        {
            _users.Remove(user);
        }
    }
}
