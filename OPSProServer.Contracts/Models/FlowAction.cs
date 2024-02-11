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
        public CanExecuteActionNotifier? CanExecuteAction { get; set; }
        public FlowActionRequest? Request { get; set; }
        public FlowAction? NextAction { get; private set; }
        public FlowContext FinalContext { get; set; }

        public delegate RuleResponse ActionNotifier(FlowArgs args);
        public delegate bool CanExecuteActionNotifier(User user, Room room, Game game, FlowAction action);

        public FlowAction(User from, User to, ActionNotifier actionNotifier, CanExecuteActionNotifier? canExecuteAction = null)
        {
            Id = Guid.NewGuid();
            FromUser = from;
            ToUser = to;
            Action = actionNotifier;
            CanExecuteAction = canExecuteAction;
            Request = null;
            NextAction = null;
            FinalContext = FlowContext.None;
        }

        public bool CanExecute(User user, Room room, Game game)
        {
            if (CanExecuteAction == null)
            {
                return true;
            }

            return CanExecuteAction(user, room, game, this);
        }

        public void DeepCopyContext()
        {
            if (NextAction != null)
            {
                var flowAction = NextAction;
                while (flowAction.NextAction != null)
                {
                    if (flowAction.NextAction.FinalContext == FlowContext.None || flowAction.NextAction.FinalContext == flowAction.FinalContext)
                    {
                        flowAction.NextAction.FinalContext = flowAction.FinalContext;
                        flowAction = flowAction.NextAction;
                    } else
                    {
                        break;
                    }
                }
            }
        }

        public void AddAfter(FlowAction action)
        {
            if (NextAction != null)
            {
                action.AddLast(NextAction);
            }

            NextAction = action;
        }

        public void AddFirst(FlowAction action)
        {
            if (NextAction != null)
            {
                action.AddLast(NextAction);
            }

            NextAction = action;

            DeepCopyContext();
        }

        public void AddLast(FlowAction action)
        {
            var flowAction = this;
            while (flowAction.NextAction != null)
            {
                flowAction = flowAction.NextAction;
            }

            flowAction.NextAction = action;

            DeepCopyContext();
        }
    }
}
