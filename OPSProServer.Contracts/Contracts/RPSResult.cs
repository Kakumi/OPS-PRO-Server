using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class RPSResult
    {
        public Dictionary<Guid, RPSChoice> Signs { get; set; }
        public Guid? Winner { get; set; }

        public RPSResult()
        {
            Signs = new Dictionary<Guid, RPSChoice>();
        }
    }
}
