namespace OPSProServer.Contracts.Models.Scripts
{
    public class CardScriptResult
    {
        public int Loaded { get; }
        public int Total { get; }

        public CardScriptResult(int loaded, int total)
        {
            Loaded = loaded;
            Total = total;
        }
    }
}
