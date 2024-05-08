using System;

namespace OPSProServer.Contracts.Hubs
{
    public interface IUserHub
    {
        /// <summary>
        /// <para>Register the user to the server.</para>
        /// <para>This method return <see cref="Guid"/> when user is registered.</para>
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Guid Register(string username);
    }
}
