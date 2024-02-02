using OPSProServer.Contracts.Models;
using OPSProServer.Models;

namespace OPSProServer.Managers
{
    public interface IFlowManager
    {
        ActionResolver? GetResolver(Guid? id);
        void Resolve(Guid id);
        void AddResolver(ActionResolver resolver);
        UserResolver? GetUserResolver(Guid? id);
        void ResolveUser(Guid id);
        void AddUserResolver(UserResolver resolver);
        FlowAction? GetFlow(Guid? id);
        FlowAction? ResolveFlow(Guid id);
        void AddFlow(FlowAction action);
    }
}
