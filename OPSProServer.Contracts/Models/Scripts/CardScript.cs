using OPSProServer.Contracts.Models;
using System.Collections.Generic;

namespace OPSProServer.Contracts.Models.Scripts
{
    public class CardScript
    {
        public virtual List<string> GetOthersColors(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new List<string>();
        }

        public virtual List<string> GetOthersName(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new List<string>();
        }

        public virtual List<string> GetOthersType(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new List<string>();
        }

        public virtual bool IsBanish(User user, PlayerGameInformation gameInfo, Game game, PlayingCard card)
        {
            return card.CardInfo.IsBanish;
        }

        public virtual bool IsBlocker(User user, PlayerGameInformation gameInfo, Game game, PlayingCard card)
        {
            return card.CardInfo.IsBlocker;
        }

        public virtual bool IsDoubleAttack(User user, PlayerGameInformation gameInfo, Game game, PlayingCard card)
        {
            return card.CardInfo.IsDoubleAttack;
        }

        public virtual bool IsRush(User user, PlayerGameInformation gameInfo, Game game, PlayingCard card)
        {
            return card.CardInfo.IsRush;
        }

        public virtual RuleResponse OnActivateCounterEvent(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnActivateEffect(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnActivateTrigger(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnAddLifeCardToHand(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnAttack(User user, PlayerGameInformation gameInfo, Game game, PlayingCard attacker, PlayingCard target)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnBlock(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnCounter(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnDonReturnedDeck(User user, PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnDonUsed(User user, PlayerGameInformation gameInfo, Game game, int amount)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnCost(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnDonRestUp(User user, PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnDrawDon(User user, PlayerGameInformation gameInfo, Game game, int amount)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnPhaseBegin(User user, PlayerGameInformation gameInfo, Game game, PhaseType phaseType)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnPhaseEnd(User user, PlayerGameInformation gameInfo, Game game, PhaseType phaseType)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnDraw(User user, PlayerGameInformation gameInfo, Game game, PlayingCard card)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnEndTurn(User user, PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnEvent(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnGetLifeCard(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnGiveDon(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnKO(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnPlay(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnRest(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnRestUp(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnStartTurn(User user, PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnSummoned(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnTrash(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public virtual RuleResponse OnTrigger(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return new RuleResponse();
        }

        public bool IsUserAction(User user, PlayerGameInformation gameInfo)
        {
            return gameInfo.User.Id == user.Id;
        }

        public bool IsSameCard(PlayingCard actionCard, PlayingCard callerCard)
        {
            return actionCard.Id == callerCard.Id;
        }
    }
}
