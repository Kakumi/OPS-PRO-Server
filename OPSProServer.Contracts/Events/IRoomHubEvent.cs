using OPSProServer.Contracts.Models;

namespace OPSProServer.Contracts.Events
{
    public interface IRoomHubEvent
    {
        Room RoomUpdated();
        void RoomDeleted();
        void RoomExcluded();
    }
}
