using System;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class UserRoom : User
    {
        public DeckInfo? Deck { get; set; }
        public RPSChoice RPSChoice { get; set; }

        public bool Ready => Deck != null;

        [JsonConstructor]
        public UserRoom(DeckInfo? deck, RPSChoice rPSChoice, Guid id, string connectionId, string username) : base(id, connectionId, username)
        {
            Deck = deck;
            RPSChoice = rPSChoice;
        }

        public UserRoom(Guid id, string connectionId, string username) : base(id, connectionId, username)
        {

        }

        public UserRoom(User user) : this(user.Id, user.ConnectionId, user.Username)
        {

        }
    }
}
