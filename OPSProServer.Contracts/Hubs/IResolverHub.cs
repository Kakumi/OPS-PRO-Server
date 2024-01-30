using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Hubs
{
    public interface IResolverHub
    {
        Task<bool> ResolveAction(Guid userId, Guid actionId, List<Guid> cards);

        Task<bool> ResolveAskAction(Guid userId, Guid actionId, bool value);
    }
}
