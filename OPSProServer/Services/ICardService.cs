using OPSProServer.Contracts.Models;

namespace OPSProServer.Services
{
    public interface ICardService
    {
        IEnumerable<CardInfo> GetCardsInfo();
        CardInfo? GetCardInfo(string id);
    }
}
