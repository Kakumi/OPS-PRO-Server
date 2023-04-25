namespace OPS_Pro_Server.Models
{
    public class User
    {
        public Guid Id { get; private set; }
        public string ConnectionId { get; private set; }
        public string UserName { get; private set; }
        public Room? CurrentRoom { get; set; }

        public User(Guid id, string connectionId, string username)
        {
            Id = id;
            ConnectionId = connectionId;
            UserName = username;
        }
    }
}
