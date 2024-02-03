using OPSProServer.Contracts.Models;
using OPSProServer.Models;

namespace OPSProServer.Managers
{
    public interface IFlowManager
    {
        FlowAction? Get(Guid? id);
        FlowAction? Resolve(Guid id);
        void Add(FlowAction action);
    }
}
