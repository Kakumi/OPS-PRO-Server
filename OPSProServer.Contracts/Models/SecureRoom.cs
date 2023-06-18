using System;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class SecureRoom
    {
        public Guid Id { get; private set; }
        public RoomState State { get; private set; }
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

        public SecureRoom(Room room)
        {
            Id = room.Id;
            State = room.State;
            Creator = room.Creator;
            Opponent = room.Opponent;
            Created = room.Created;
            UsePassword = room.UsePassword;
            Description = room.Description;
        }
    }
}
