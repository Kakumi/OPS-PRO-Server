using OPSProServer.Attributes;
using OPSProServer.Contracts.Models;

namespace OPSProServer.Models.Scripts.OP01
{
    [CardScript("OP01", "001")]
    public class OP01_001 : DefaultCardScript
    {
        public override void OnGiveDon(User user, Game game, PlayingCard playingCard)
        {
            if (playingCard.DonCard >= 1 && !playingCard.HasOncePerTurn())
            {
                var gameInfo = game.GetMyPlayerInformation(user.Id);
                if (gameInfo != null)
                {
                    playingCard.SetOncePerTurnTag();
                    foreach (var character in gameInfo.GetCharacters())
                    {
                        character.PowerModifier.Add(new ValueModifier(ModifierDuration.Turn, 1000));
                    }
                }
            }
        }
    }
}
