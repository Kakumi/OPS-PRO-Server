using OPSProServer.Contracts.Models;
using OPSProServer.Models;

namespace OPSProServer.Managers
{
    public class FlowManager : IFlowManager
    {
        private readonly List<FlowAction> _flows;

        public FlowManager()
        {
            _flows = new List<FlowAction>();
        }

        public FlowAction? Get(Guid? id)
        {
            return _flows.FirstOrDefault(x => x.Id == id);
        }

        public FlowAction? Resolve(Guid id)
        {
            var flow = _flows.FirstOrDefault(x => x.Id == id);
            if (flow != null)
            {
                _flows.RemoveAll(x => x.Id == id);
                return flow.NextAction;
            }

            return null;
        }

        public void Add(FlowAction action)
        {
            var flow = Get(action.Id);
            if (flow == null)
            {
                _flows.Add(action);
            }
        }
    }
}
