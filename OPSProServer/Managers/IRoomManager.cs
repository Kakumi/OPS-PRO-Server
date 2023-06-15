using OPSProServer.Models;

namespace OPSProServer.Managers
{
    public interface IRoomManager
    {
        List<Room> GetRooms();
        Room? GetRoom(Guid id);
        Room? GetRoom(User user);
        void AddRoom(Room room);
        void RemoveRoom(Guid id);
    }
}
