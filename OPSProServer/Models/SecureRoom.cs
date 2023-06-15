namespace OPSProServer.Models
{
    public class SecureRoom
    {
        public Guid Id { get; }
        public RoomState State { get; }
        public UserRoom Creator { get; }
        public UserRoom? Opponent { get; }
        public DateTime Created { get; }
        public bool UsePassword { get; }
        public string? Description { get; }

        internal SecureRoom(Room room)
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
