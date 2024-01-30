using OPSProServer.Contracts.Models;
using OPSProServer.Models;

namespace OPSProServer.Services
{
    public interface IGameRuleService
    {
        bool CanAttack(PlayingCard? card, User user, Game game);
        AttackResult Attack(User user, Game game, Guid attacker, Guid target);
        RuleResponse GiveDon(User user, Game game, Guid cardId);
        List<PlayingCard> GetAttackableCards(User user, Game game);
        List<PlayingCard> GetCounterCards(User user, Game game);
        List<PlayingCard> GetBlockerCards(User user, Game game);
        bool CanSummon(User user, Game game, Guid cardId);
        RuleResponse Summon(User user, Game game, Guid cardId, Guid replaceId = default);
        RuleResponse UseCounters(User user, Game game, Guid fromCardId, List<Guid> cardsId);
    }
}
