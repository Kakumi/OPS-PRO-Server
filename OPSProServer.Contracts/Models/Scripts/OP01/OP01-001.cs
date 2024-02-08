using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Models.Scripts;

namespace OPSProServer.Contracts.Models.Scripts.OP01
{
    [CardScript("OP01", "001")]
    public class OP01_001 : CardScript
    {
        public override RuleResponse OnGiveDon(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            var response = new RuleResponse();

            if (IsUserAction(user, gameInfo) && IsSameCard(actionCard, callerCard)) {
                if (actionCard.DonCard >= 1 && !actionCard.HasOncePerTurn())
                {
                    response.FlowResponses.Add(new FlowResponseMessage("GAME_GENERIC_CARD_EFFECT", actionCard.CardInfo.Name));
                    actionCard.SetOncePerTurnTag();
                    foreach (var character in gameInfo.GetCharacters())
                    {
                        character.PowerModifier.Add(new ValueModifier(ModifierDuration.Turn, 1000));
                    }
                }
            }

            return response;
        }

        public override RuleResponse OnCounter(User user, PlayerGameInformation gameInfo, Game game, PlayingCard callerCard, PlayingCard actionCard)
        {
            return base.OnCounter(user, gameInfo, game, callerCard, actionCard);
        }
    }
}
