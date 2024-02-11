using System.Linq;

namespace OPSProServer.Contracts.Models
{
    public class DonPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.Don;

        public PhaseState State { get; set; }

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See;
        }

        public IPhase NextPhase()
        {
            return new MainPhase();
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

            int amount;
            if (game.FirstToPlay == game.PlayerTurn && game.Turn == 1)
            {
                amount = 1;
            }
            else
            {
                amount = 2;
            }

            playerInfo.DrawDonCard(amount);
            ruleResponse.Add(playerInfo.GetBoard().Select(x => x.Script.OnDrawDon(playerInfo.User, playerInfo, game, amount)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnDrawDon(playerInfo.User, opponentInfo, game, amount)));

            return ruleResponse;
        }

        public bool IsAutoNextPhase()
        {
            return true;
        }
    }
}
