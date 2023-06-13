namespace OPSProServer.Contracts.Contracts
{
    public class User
    {
        public Guid Id { get; }
        public string ConnectionId { get; }
        public string Username { get; }
        public DateTime Created { get; }

        public User(Guid id, string connectionId, string username)
        {
            Id = id;
            ConnectionId = connectionId;
            Username = username;
            Created = DateTime.Now;
        }
    }
}
