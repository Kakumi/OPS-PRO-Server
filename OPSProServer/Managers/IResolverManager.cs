using OPSProServer.Contracts.Models;

namespace OPSProServer.Managers
{
    public interface IResolverManager
    {
        ActionResolver? GetResolver(Guid? id);
        void Resolve(Guid id);
        void AddResolver(ActionResolver resolver);
        UserResolver? GetUserResolver(Guid? id);
        void ResolveUser(Guid id);
        void AddUserResolver(UserResolver resolver);
    }
}
