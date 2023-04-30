﻿using OPS_Pro_Server.Models;
using OPSProServer.Contracts.Contracts;

namespace OPS_Pro_Server.Managers
{
    public interface IRoomManager
    {
        IReadOnlyList<Room> GetRooms();
        Room? GetRoom(Guid id);
        Room? GetRoom(User user);
        void AddRoom(Room room);
        void RemoveRoom(Guid id);
    }
}