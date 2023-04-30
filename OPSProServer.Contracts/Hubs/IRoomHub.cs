﻿using OPSProServer.Contracts.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Hubs
{
    public interface IRoomHub
    {
        Task<bool> CreateRoom(Guid userId, string? password, string? description);

        IReadOnlyList<Room> GetRooms();

        Task<bool> JoinRoom(Guid userId, Guid roomId, string? password);

        Task<bool> LeaveRoom(Guid userId);
    }
}