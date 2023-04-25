using OPS_Pro_Server.Models;

namespace OPS_Pro_Server.Managers
{
    public class RoomManager : IRoomManager
    {
        private readonly List<Room> _rooms;

        public RoomManager()
        {
            _rooms = new List<Room>();
        }

        public IReadOnlyList<Room> GetRooms()
        {
            return _rooms;
        }

        public Room? GetRoom(Guid id)
        {
            return _rooms.FirstOrDefault(r => r.Id == id);
        }

        public Room? GetRoom(User user)
        {
            return user.CurrentRoom;
        }

        public void AddRoom(Room room)
        {
            if (!_rooms.Any(x => x.Id == room.Id))
            {
                _rooms.Add(room);
            }
        }

        public bool IsJoinable(Room room, User user, string? password)
        {
            return room.Opponent == null && room.Creator.Id != user.Id && room.Password == password;
        }
    }
}
