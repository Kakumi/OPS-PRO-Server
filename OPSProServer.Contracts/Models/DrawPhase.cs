using System.Linq;

namespace OPSProServer.Contracts.Models
{
    public class DrawPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.Draw;

        public PhaseState State { get; set; }

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See;
        }

        public IPhase NextPhase()
        {
            return new DonPhase();
        }

        public RuleResponse OnPhaseEnded(PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public RuleResponse OnPhaseStarted(PlayerGameInformation gameInfo, Game game)
        {
            var ruleResponse = new RuleResponse();

            var playerInfo = game.GetCurrentPlayerGameInformation();
            var opponentInfo = game.GetOpponentPlayerInformation(playerInfo.User.Id);

            if (game.FirstToPlay != playerInfo.User.Id || game.Turn != 1)
            {
                var cards = playerInfo.DrawCard();
                ruleResponse.Add(playerInfo.GetBoard().Select(x => x.Script.OnDraw(playerInfo.User, playerInfo, game, cards.First())));
                ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnDraw(playerInfo.User, opponentInfo, game, cards.First())));
            }

            return ruleResponse;
        }

        public bool IsAutoNextPhase()
        {
            return true;
        }
    }
}