using Microsoft.AspNetCore.SignalR;
using OPSProServer.Contracts.Models;
using OPSProServer.Hubs;

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

        public virtual void OnGiveDon(User user, Game game, PlayingCard playingCard) { }

        public virtual void OnTrash(User user, Game game, PlayingCard playingCard) { }

        public virtual Contracts.Models.RuleResponse OnTrigger(User user, Game game, PlayingCard playingCard)
        {
            return new Contracts.Models.RuleResponse();
        }
    }
}
