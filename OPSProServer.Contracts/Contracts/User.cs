namespace OPSProServer.Contracts.Contracts
{
    public class User
    {
        public Guid Id { get; set; }
        public string ConnectionId { get; set; }
        public string Username { get; set; }

        public User(Guid id, string connectionId, string username)
        {
            Id = id;
            ConnectionId = connectionId;
            Username = username;
        }

        public User() { }
    }
}
