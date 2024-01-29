using OPSProServer.Contracts.Models;

namespace OPSProServer.Managers
{
    public class ResolverManager : IResolverManager
    {
        private readonly List<ActionResolver> _resolvers;
        private readonly List<UserResolver> _userResolvers;

        public ResolverManager()
        {
            _resolvers = new List<ActionResolver>();
            _userResolvers = new List<UserResolver>();
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
    }
}
