using OPSProServer.Contracts.Models;

namespace OPSProServer.Managers
{
    public class RoomManager : IRoomManager
    {
        private readonly List<Room> _rooms;

        public RoomManager()
        {
            _rooms = new List<Room>();
#if DEBUG
            _rooms.Add(new Room(new User("test", "Server"), "Server test room"));

            _rooms.Add(new Room(new User("test", "Server"), "Server test room (password)", "admin"));

            var roomFull = new Room(new User("test", "Server"), "Server test room (full)");
            roomFull.SetOpponent(new User("test2", "Server 2"));
            _rooms.Add(roomFull);
#endif
        }

        public List<Room> GetRooms()
        {
            return _rooms;
        }

        public Room? GetRoom(Guid id)
        {
            return _rooms.FirstOrDefault(r => r.Id == id);
        }

        public Room? GetRoom(User user)
        {
            return _rooms.FirstOrDefault(x => x.Creator.Id == user.Id || x.Opponent?.Id == user.Id);
        }

        public void AddRoom(Room room)
        {
            if (!_rooms.Any(x => x.Id == room.Id))
            {
                _rooms.Add(room);
            }
        }

        public void RemoveRoom(Guid id)
        {
            _rooms.RemoveAll(x => x.Id == id);
        }
    }
}
