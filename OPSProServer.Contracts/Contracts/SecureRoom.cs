using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class SecureRoom
    {
        public Guid Id { get; private set; }
        public RoomState State { get; set; }
        public UserRoom Creator { get; private set; }
        public UserRoom? Opponent { get; private set; }
        public DateTime Created { get; private set; }
        public bool UsePassword { get; private set; }
        public string? Description { get; private set; }

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
