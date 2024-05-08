using System.Linq;

namespace OPSProServer.Contracts.Models
{
    public class EndPhase : IPhase
    {
        public  PhaseType PhaseType => PhaseType.End;

        public  PhaseState State { get; set; }

        public bool IsActionAllowed(CardSource source, CardAction action)
        {
            return action == CardAction.See;
        }

        public IPhase NextPhase()
        {
            return new OpponentPhase(false);
        }

        public RuleResponse OnPhaseEnded(PlayerGameInformation gameInfo, Game game)
        {
            var response = new RuleResponse();
            var playerInfo = game.GetCurrentPlayerGameInformation();
            var opponentInfo = game.GetOpponentPlayerInformation(playerInfo.User.Id);

            playerInfo.GetCharacters().ForEach(x =>
            {
                x.RemoveStatDuration(ModifierDuration.Turn);
            });

            playerInfo.Leader.RemoveStatDuration(ModifierDuration.Turn);

            response.Add(playerInfo.GetBoard().Select(x => x.Script.OnEndTurn(playerInfo.User, playerInfo, game)));
            response.Add(opponentInfo.GetBoard().Select(x => x.Script.OnEndTurn(playerInfo.User, opponentInfo, game)));

            return response;
        }

        public RuleResponse OnPhaseStarted(PlayerGameInformation gameInfo, Game game)
        {
            return new RuleResponse();
        }

        public bool IsAutoNextPhase()
        {
            return true;
        }
    }
}