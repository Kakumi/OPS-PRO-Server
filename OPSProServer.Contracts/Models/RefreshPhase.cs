using OPSProServer.Contracts.Models;
using System.Linq;

namespace OPSProServer.Contracts.Models
{
    public class RefreshPhase : IPhase
    {
        public PhaseType PhaseType => PhaseType.Refresh;

        public PhaseState State { get; set; }

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See;
        }

        public IPhase NextPhase()
        {
            return new DrawPhase();
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

            ruleResponse.Add(playerInfo.GetBoard().Select(x => x.Script.OnStartTurn(playerInfo.User, playerInfo, game)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnStartTurn(playerInfo.User, opponentInfo, game)));

            if (game.FirstToPlay == game.PlayerTurn)
            {
                game.IncrementTurn();
            }

            playerInfo.IncrementCardsTurn();
            playerInfo.UnrestCostDeck();

            ruleResponse.Add(playerInfo.GetBoard().Select(x => x.Script.OnDonRestUp(playerInfo.User, playerInfo, game)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(x => x.Script.OnDonRestUp(playerInfo.User, opponentInfo, game)));

            playerInfo.GetCharacters().ForEach(x =>
            {
                x.DonCard = 0;
                x.Rested = false;
                x.RemoveStatDuration(ModifierDuration.OpponentTurn);

                ruleResponse.Add(playerInfo.GetBoard().Select(y => y.Script.OnRestUp(playerInfo.User, playerInfo, game, y, x)));
                ruleResponse.Add(opponentInfo.GetBoard().Select(y => y.Script.OnRestUp(playerInfo.User, opponentInfo, game, y, x)));
            });

            playerInfo.Leader.DonCard = 0;
            playerInfo.Leader.Rested = false;

            ruleResponse.Add(playerInfo.GetBoard().Select(y => y.Script.OnRestUp(playerInfo.User, playerInfo, game, y, playerInfo.Leader)));
            ruleResponse.Add(opponentInfo.GetBoard().Select(y => y.Script.OnRestUp(playerInfo.User, opponentInfo, game, y, playerInfo.Leader)));

            playerInfo.Leader.RemoveStatDuration(ModifierDuration.OpponentTurn);

            return ruleResponse;
        }

        public bool IsAutoNextPhase()
        {
            return true;
        }
    }
}