using OPS_Pro_Server.Models;

namespace OPS_Pro_Server.Managers
{
    public interface IRoomManager
    {
        IReadOnlyList<Room> GetRooms();
        Room? GetRoom(Guid id);
        Room? GetRoom(User user);
        void AddRoom(Room room);
        bool IsJoinable(Room room, User user, string? password);
    }
}
