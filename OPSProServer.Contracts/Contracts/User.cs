namespace OPSProServer.Contracts.Contracts
{
    public class User
    {
        public Guid Id { get; }
        public string ConnectionId { get; }
        public string Username { get; }
        public DateTime Created { get; }

        public User(string connectionId, string username)
        {
            Id = Guid.NewGuid();
            ConnectionId = connectionId;
            Username = username;
            Created = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Username} ({Id})";
        }
    }
}
