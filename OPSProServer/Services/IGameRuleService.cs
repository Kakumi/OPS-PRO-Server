using OPSProServer.Contracts.Models;
using OPSProServer.Models;

namespace OPSProServer.Services
{
    public interface IGameRuleService
    {
        bool CanAttack(PlayingCard? card, User user, Room room, Game game);
        AttackResult Attack(User user, Room room, Game game, Guid attacker, Guid target);
        RuleResponse GiveDon(User user, Room room, Game game, Guid cardId);
        List<PlayingCard> GetAttackableCards(User user, Room room, Game game);
        List<PlayingCard> GetCounterCards(User user, Room room, Game game);
        List<PlayingCard> GetBlockerCards(User user, Room room, Game game);
        bool CanSummon(User user, Room room, Game game, Guid cardId);
        RuleResponse Summon(User user, Room room, Game game, Guid cardId, Guid replaceId = default);
        RuleResponse UseCounters(User user, Room room, Game game, Guid fromCardId, List<Guid> cardsId);
    }
}
