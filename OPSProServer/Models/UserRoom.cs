namespace OPSProServer.Models
{
    public class UserRoom : User
    {
        public DeckInfo? Deck { get; internal set; }
        public RPSChoice RPSChoice { get; internal set; }

        public bool Ready => Deck != null;

        internal UserRoom(string connectionId, string username) : base(connectionId, username)
        {

        }

        internal UserRoom(User user) : this(user.ConnectionId, user.Username)
        {

        }
    }
}
