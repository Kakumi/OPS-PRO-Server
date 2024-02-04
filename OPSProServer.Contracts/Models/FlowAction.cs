using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public class FlowAction
    {
        public Guid Id { get; }
        public User FromUser { get; }
        public User ToUser { get; }
        public Guid? FromCardId { get; set; }
        public Guid? ToCardId { get; set; }
        public ActionNotifier Action { get; }
        public FlowActionRequest? Request { get; set; }
        public FlowAction? NextAction { get; private set; }
        public FlowContext FinalContext { get; set; }

        public delegate RuleResponse ActionNotifier(FlowArgs args);

        public FlowAction(User from, User to, ActionNotifier actionNotifier)
        {
            Id = Guid.NewGuid();
            FromUser = from;
            ToUser = to;
            Action = actionNotifier;
            Request = null;
            NextAction = null;
            FinalContext = FlowContext.None;
        }

        public void AddFirst(FlowAction action)
        {
            if (NextAction != null)
            {
                action.AddLast(NextAction);
            }

            NextAction = action;
        }

        public void AddLast(FlowAction action)
        {
            var flowAction = this;
            while (flowAction.NextAction != null)
            {
                flowAction = flowAction.NextAction;
            }

            flowAction.NextAction = action;
        }
    }
}
