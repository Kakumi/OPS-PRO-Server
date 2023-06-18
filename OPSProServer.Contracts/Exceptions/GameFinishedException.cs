namespace OPSProServer.Contracts.Exceptions
{
    public class GameFinishedException : OPSException
    {
        public GameFinishedException() : base("Game is done")
        {
        }
    }
}
