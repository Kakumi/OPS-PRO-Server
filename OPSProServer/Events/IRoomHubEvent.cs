using OPSProServer.Models;

namespace OPSProServer.Events
{
    public interface IRoomHubEvent
    {
        Room RoomUpdated();
        void RoomDeleted();
        void RoomExcluded();
    }
}
