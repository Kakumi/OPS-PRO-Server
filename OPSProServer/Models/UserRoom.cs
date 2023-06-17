namespace OPSProServer.Models
{
    public class UserRoom : User
    {
        public DeckInfo? Deck { get; internal set; }
        public RPSChoice RPSChoice { get; internal set; }

        public bool Ready => Deck != null;

        internal UserRoom(Guid id, string connectionId, string username) : base(id, connectionId, username)
        {

        }

        internal UserRoom(User user) : this(user.Id, user.ConnectionId, user.Username)
        {

        }
    }
}
