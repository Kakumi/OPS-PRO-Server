using OPSProServer.Contracts.Models;

namespace OPSProServer.Services
{
    public interface ICardService
    {
        IEnumerable<CardInfo> GetCardsInfo();
        CardInfo? GetCardInfo(string id);
        ICardScript? GetCardScript(PlayingCard playingCard);

        public bool IsBlocker(PlayingCard playingCard, User user, Game game);

        public bool IsRusher(PlayingCard playingCard, User user, Game game);

        public bool IsDoubleAttack(PlayingCard playingCard, User user, Game game);

        public bool IsBanish(PlayingCard playingCard, User user, Game game);

        public bool IsTrigger(PlayingCard playingCard, User user, Game game);
    }
}
