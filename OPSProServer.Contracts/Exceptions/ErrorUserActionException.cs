using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Exceptions
{
    public class ErrorUserActionException : OPSException
    {
        public Guid UserId { get; }
        public string[] Args { get; }
        public ErrorUserActionException(Guid userId, string? message, params string[] args) : base(message)
        {
            UserId = userId;
            Args = args;
        }
    }
}
