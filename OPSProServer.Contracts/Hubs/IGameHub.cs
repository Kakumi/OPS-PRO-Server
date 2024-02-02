using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Events;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OPSProServer.Contracts.Hubs
{
    public interface IGameHub
    {
        /// <summary>
        /// <para>Tell the server that the Rock Paper Scissors should start.</para>
        /// <para>This method will fire <see cref="IGameHubEvent.RockPaperScissorsStarted"/>.</para>
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        Task<bool> LaunchRockPaperScissors(Guid roomId);

        /// <summary>
        /// <para>Set your Rock Paper Scissors choice and wait for both players.</para>
        /// <para>This method will fire <see cref="IGameHubEvent.RPSExecuted"/> when both player made a choice.</para>
        /// <para>This method will fire <see cref="IGameHubEvent.ChooseFirstPlayerToPlay"/> to the winner.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="rps"></param>
        /// <returns></returns>
        Task<bool> SetRockPaperScissors(RPSChoice rps);

        /// <summary>
        /// <para>Tell the server that the game should start.</para>
        /// <para>This method will fire <see cref="IGameHubEvent.GameStarted"/>.</para>
        /// <para>This method will fire <see cref="IGameHubEvent.BoardUpdated"/>.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userToStart"></param>
        /// <returns></returns>
        Task<bool> LaunchGame(Guid userToStart);

        /// <summary>
        /// <para>Tell the server the user want to go to next phase.</para>
        /// <para>This method will fire <see cref="IGameHubEvent.BoardUpdated"/>.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> NextPhase();

        /// <summary>
        /// <para>Ask the server which cards can the user attack.</para>
        /// <para>This method will fire <see cref="IGameHubEvent.BoardUpdated"/>.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> GetAttackableCards(Guid attacker);

        Task<bool> Attack(Guid attacker, Guid target);

        Task<bool> GiveDonCard(Guid characterCardId);

        Task<bool> ActivateCardEffect(Guid characterCardId);

        Task<bool> Summon(Guid cardId);

        Task<bool> ResolveFlow(FlowActionResponse response);
    }
}
