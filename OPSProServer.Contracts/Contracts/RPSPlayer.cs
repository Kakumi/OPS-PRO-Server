using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class RPSPlayer
    {
        public Guid UserId { get; }
        public RPSChoice Choice { get; set; }

        public RPSPlayer(Guid id)
        {
            UserId = id;
        }
    }
}
