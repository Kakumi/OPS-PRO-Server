using OPSProServer.Contracts.Models;

namespace OPSProServer.Contracts.Events
{
    public interface IRoomHubEvent
    {
        /// <summary>
        /// Event when a room is updated (ready, join, left)
        /// </summary>
        /// <returns></returns>
        SecureRoom RoomUpdated();

        /// <summary>
        /// Event when a room is deleted
        /// </summary>
        void RoomDeleted();

        /// <summary>
        /// Event when a user is kick from the room
        /// </summary>
        void RoomExcluded();
    }
}
