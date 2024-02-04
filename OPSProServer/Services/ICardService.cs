using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Models.Scripts;

namespace OPSProServer.Services
{
    public interface ICardService
    {
        IEnumerable<CardInfo> GetCardsInfo();
        CardInfo? GetCardInfo(string id);
    }
}
