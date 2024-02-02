using OPSProServer.Contracts.Models;
using OPSProServer.Models;

namespace OPSProServer.Managers
{
    public class FlowManager : IFlowManager
    {
        private readonly List<ActionResolver> _resolvers;
        private readonly List<UserResolver> _userResolvers;
        private readonly List<FlowAction> _flows;

        public FlowManager()
        {
            _resolvers = new List<ActionResolver>();
            _userResolvers = new List<UserResolver>();
            _flows = new List<FlowAction>();
        }

        public void AddResolver(ActionResolver resolver)
        {
            if (GetResolver(resolver.Id) == null)
            {
                _resolvers.Add(resolver);
            }
        }

        public ActionResolver? GetResolver(Guid? id)
        {
            if (id == null)
            {
                return null;
            }

            return _resolvers.FirstOrDefault(x => x.Id == id);
        }

        public void Resolve(Guid id)
        {
            _resolvers.RemoveAll(x => x.Id == id);
        }

        public void AddUserResolver(UserResolver resolver)
        {
            if (GetUserResolver(resolver.Id) == null)
            {
                _userResolvers.Add(resolver);
            }
        }

        public UserResolver? GetUserResolver(Guid? id)
        {
            if (id == null)
            {
                return null;
            }

            return _userResolvers.FirstOrDefault(x => x.Id == id);
        }

        public void ResolveUser(Guid id)
        {
            _userResolvers.RemoveAll(x => x.Id == id);
        }

        public FlowAction? GetFlow(Guid? id)
        {
            return _flows.FirstOrDefault(x => x.Id == id);
        }

        public FlowAction? ResolveFlow(Guid id)
        {
            var flow = _flows.FirstOrDefault(x => x.Id == id);
            if (flow != null)
            {
                _flows.RemoveAll(x => x.Id == id);
                return flow.NextAction;
            }

            return null;
        }

        public void AddFlow(FlowAction action)
        {
            var flow = GetFlow(action.Id);
            if (flow == null)
            {
                _flows.Add(action);
            }
        }
    }
}
