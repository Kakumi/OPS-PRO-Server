using System;

namespace OPSProServer.Contracts.Exceptions
{
    public class OPSException : Exception
    {
        public OPSException(string? message) : base(message)
        {
        }

        public OPSException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
