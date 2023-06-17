namespace OPSProServer.Exceptions
{
    public class UnauthorizedOperationException : OPSException
    {
        internal UnauthorizedOperationException(string? operation) : base($"The operation {operation} is not authorized.")
        {
        }
    }
}
