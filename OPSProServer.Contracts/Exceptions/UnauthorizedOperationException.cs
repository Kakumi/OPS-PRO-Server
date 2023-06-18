namespace OPSProServer.Contracts.Exceptions
{
    public class UnauthorizedOperationException : OPSException
    {
        public UnauthorizedOperationException(string? operation) : base($"The operation {operation} is not authorized.")
        {
        }
    }
}
