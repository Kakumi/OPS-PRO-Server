using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Models
{
    public interface ICardScript
    {
        public PlayingCard PlayingCard { get; }

        public bool IsBlocker(User user, Game game);

        public bool IsRusher(User user, Game game);

        public bool IsDoubleAttack(User user, Game game);

        public bool IsBanish(User user, Game game);

        public bool IsTrigger(User user, Game game);

        ///// <summary>
        ///// <para>Tell if the card can use his On Play event</para>
        ///// </summary>
        ///// <returns></returns>
        //bool CanFireOnPlay();

        ///// <summary>
        ///// <para>Tell if the card can use his Trigger event.</para>
        ///// </summary>
        ///// <returns></returns>
        //bool CanFireTrigger();

        ///// <summary>
        ///// <para>Tell if the card can call Activate event.</para>
        ///// </summary>
        ///// <returns></returns>
        //bool CanFireActivate();

        ///// <summary>
        ///// <para>Tell if the card can call because of opponent action.</para>
        ///// </summary>
        ///// <param name="trigger"></param>
        ///// <returns></returns>
        //bool CanFireOpponentTurn(PlayingCard? opponentCard, PlayingCard? playerCard);

        ///// <summary>
        ///// <para>Tell if the card can be killed by attacker.</para>
        ///// <para>This method is partically used for card that cannot be defeated by a special card.</para>
        ///// </summary>
        ///// <param name="playingCard"></param>
        ///// <returns></returns>
        //bool CanBeKilledBy(PlayingCard attacker);

        //List<string> GetOthersName();
        //List<string> GetOthersType();

        //public void OnPlay();
        //public void OnTrigger();
        //public void OnActivate();
        //public void OnOpponentTurn();
        public void OnGiveDon(User user, Game game);
    }
}
