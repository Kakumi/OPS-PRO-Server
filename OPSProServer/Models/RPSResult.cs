namespace OPSProServer.Models
{
    public class RPSResult
    {
        public Dictionary<Guid, RPSChoice> Signs { get; private set; }
        public Guid? Winner { get; }

        internal RPSResult(Guid? winner) : this(winner, new Dictionary<Guid, RPSChoice>())
        {
        }

        internal RPSResult(Guid? winner, Dictionary<Guid, RPSChoice> dic)
        {
            Winner = winner;
            Signs = dic;
        }
    }
}
