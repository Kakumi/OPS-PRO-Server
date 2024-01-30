using OPSProServer.Contracts.Models;

namespace OPSProServer.Models.Scripts
{
    public class DefaultCardScript : ICardScript
    {
        public virtual bool IsBanish(User user, Game game, PlayingCard playingCard)
        {
            return playingCard.CardInfo.IsBanish;
        }

        public virtual bool IsBlocker(User user, Game game, PlayingCard playingCard)
        {
            return playingCard.CardInfo.IsBlocker;
        }

        public virtual bool IsDoubleAttack(User user, Game game, PlayingCard playingCard)
        {
            return playingCard.CardInfo.IsDoubleAttack;
        }

        public virtual bool IsRusher(User user, Game game, PlayingCard playingCard)
        {
            return playingCard.CardInfo.IsRush;
        }

        public virtual bool IsTrigger(User user, Game game, PlayingCard playingCard)
        {
            return playingCard.CardInfo.IsTrigger;
        }

        public virtual void OnGiveDon(User user, Game game, PlayingCard playingCard) { }

        public virtual void OnTrash(User user, Game game, PlayingCard playingCard) { }
    }
}
