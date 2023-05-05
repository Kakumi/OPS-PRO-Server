﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Hubs
{
    public interface IGameHub
    {
        Task<bool> LaunchGame(Guid roomId);
    }
}
