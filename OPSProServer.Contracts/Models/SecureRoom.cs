using System;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class SecureRoom
    {
        public Guid Id { get; private set; }
        public RoomState State { get; protected set; }
        public UserRoom Creator { get; private set; }
        public UserRoom? Opponent { get; private set; }
        public DateTime Created { get; private set; }
        public bool UsePassword { get; private set; }
        public string? Description { get; private set; }

        [JsonConstructor]
        public SecureRoom(Guid id, RoomState state, UserRoom creator, UserRoom? opponent, DateTime created, bool usePassword, string? description)
        {
            Id = id;
            State = state;
            Creator = creator;
            Opponent = opponent;
            Created = created;
            UsePassword = usePassword;
            Description = description;
        }

        public SecureRoom(User user, bool usePassword, string? description = null)
        {
            Id = Guid.NewGuid();
            State = RoomState.Created;
            Creator = new UserRoom(user);
            Created = DateTime.Now;
            Description = description;
            UsePassword = usePassword;
        }

        public bool IsInside(User user)
        {
            return Creator.Id == user.Id || Opponent?.Id == user.Id;
        }

        public bool CanStart()
        {
            return Opponent != null && Opponent.Ready && Creator != null && Creator.Ready;
        }

        public void SetOpponent(User? opponent)
        {
            if (opponent != null)
            {
                Opponent = new UserRoom(opponent);
            }
            else
            {
                Opponent = null;
            }
        }

        public User? GetOpponent(Guid userId)
        {
            if (userId == Creator.Id)
            {
                return Opponent;
            }

            return Creator;
        }

        public User? GetOpponent(User user)
        {
            if (user.Id == Creator.Id)
            {
                return Opponent;
            }

            return Creator;
        }

        public UserRoom? GetUserRoom(User user)
        {
            if (Creator.Id == user.Id)
            {
                return Creator;
            }
            else if (Opponent != null)
            {
                return Opponent;
            }

            return null;
        }
    }
}
