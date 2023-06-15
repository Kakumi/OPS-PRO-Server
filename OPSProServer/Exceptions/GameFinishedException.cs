namespace OPSProServer.Exceptions
{
    public class GameFinishedException : OPSException
    {
        internal GameFinishedException() : base("Game is done")
        {
        }
    }
}
