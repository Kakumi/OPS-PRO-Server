using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;

namespace OPS_Pro_Server.Managers
{
    public class RoomManager : IRoomManager
    {
        private readonly List<Room> _rooms;

        public RoomManager()
        {
            _rooms = new List<Room>();
#if DEBUG
            _rooms.Add(new Room()
            {
                Id = Guid.NewGuid(),
                Creator = new User()
                {
                    Id = Guid.NewGuid(),
                    ConnectionId = "test",
                    Username = "Server"
                },
                Opponent = null,
                Password = null,
                Created = DateTime.Now,
                Description = "Room created from the server.",
                CreatorReady = false,
                OpponentReady = false,
                UsePassword = false,
            });
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
