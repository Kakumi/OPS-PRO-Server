using System;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class User
    {
        public Guid Id { get; private set; }
        public string ConnectionId { get; private set; }
        public string Username { get; private set; }
        public DateTime Created { get; private set; }

        [JsonConstructor]
        public User(Guid id, string connectionId, string username, DateTime created) : this(id, connectionId, username)
        {
            Created = created;
        }

        public User(string connectionId, string username)
        {
            Id = Guid.NewGuid();
            ConnectionId = connectionId;
            Username = username;
            Created = DateTime.Now;
        }

        public User(Guid id, string connectionId, string username)
        {
            Id = id;
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
