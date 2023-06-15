namespace OPSProServer.Exceptions
{
    public class OPSException : Exception
    {
        internal OPSException(string? message) : base(message)
        {
        }

        internal OPSException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
