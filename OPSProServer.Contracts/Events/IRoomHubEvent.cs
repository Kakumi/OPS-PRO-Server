using OPSProServer.Contracts.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Events
{
    public interface IRoomHubEvent
    {
        Room RoomUpdated();
        void RoomDeleted();
        void RoomExcluded();
    }
}
