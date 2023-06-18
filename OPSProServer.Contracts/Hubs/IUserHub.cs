using System;

namespace OPSProServer.Contracts.Hubs
{
    public interface IUserHub
    {
        Guid Register(string username);
    }
}
