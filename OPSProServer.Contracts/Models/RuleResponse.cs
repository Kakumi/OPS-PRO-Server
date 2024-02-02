using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class RuleResponse
    {
        public FlowAction? FlowAction { get; set; }
        public List<FlowResponseMessage> FlowResponses { get; set; }

        public RuleResponse()
        {
            FlowAction = null;
            FlowResponses = new List<FlowResponseMessage>();
        }
    }
}
