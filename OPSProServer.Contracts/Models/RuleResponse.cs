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
        public bool PriorityFlowAction { get; set; }
        public List<FlowResponseMessage> FlowResponses { get; set; }

        public RuleResponse()
        {
            FlowAction = null;
            PriorityFlowAction = true;
            FlowResponses = new List<FlowResponseMessage>();
        }

        public void Add(RuleResponse ruleResponse)
        {
            if (ruleResponse.FlowAction != null)
            {
                if (FlowAction == null)
                {
                    FlowAction = ruleResponse.FlowAction;
                } else if (PriorityFlowAction)
                {
                    FlowAction.AddFirst(ruleResponse.FlowAction);
                } else
                {
                    FlowAction.AddLast(ruleResponse.FlowAction);
                }
            }

            FlowResponses.AddRange(ruleResponse.FlowResponses);
        }

        public void Add(IEnumerable<RuleResponse> rulesResponse)
        {
            foreach(var ruleResponse in rulesResponse)
            {
                Add(ruleResponse);
            }
        }
    }
}
