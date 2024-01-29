using OPSProServer.Attributes;
using OPSProServer.Contracts.Models;

namespace OPSProServer.Models.Scripts.OP01
{
    [CardScript("OP01", "001")]
    public class OP01_001 : ICardScript
    {
        public PlayingCard PlayingCard { get; private set; }

        public OP01_001(PlayingCard playingCard) 
        {
            PlayingCard = playingCard;
        }

        public bool IsBanish(User user, Game game)
        {
            return false;
        }

        public bool IsBlocker(User user, Game game)
        {
            return false;
        }

        public bool IsDoubleAttack(User user, Game game)
        {
            return false;
        }

        public bool IsRusher(User user, Game game)
        {
            return false;
        }

        public bool IsTrigger(User user, Game game)
        {
            return false;
        }

        public void OnGiveDon(User user, Game game)
        {
            var gameInfo = game.GetMyPlayerInformation(user.Id);
            if (gameInfo != null)
            {
                foreach(var character in gameInfo.GetCharacters())
                {
                    character.PowerModifier.Add(new KeyValuePair<ModifierDuration, int>(ModifierDuration.Turn, 1000));
                }
            }
        }
    }
}
