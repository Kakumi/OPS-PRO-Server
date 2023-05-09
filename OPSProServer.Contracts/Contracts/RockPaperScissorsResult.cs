using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Contracts
{
    public class RockPaperScissorsResult
    {
        public Dictionary<Guid, RockPaperScissors> Signs { get; set; }
        public Guid? Winner { get; set; }

        public RockPaperScissorsResult()
        {
            Signs = new Dictionary<Guid, RockPaperScissors>();
        }
    }
}
