using OPSProServer.Contracts.Hubs;
using OPSProServer.Contracts.Models;
using System;
using System.Collections.Generic;

namespace OPSProServer.Contracts.Events
{
    public interface IGameHubEvent
    {
        /// <summary>
        /// Event when a Rock Paper Scissors start and need player input.
        /// </summary>
        void RockPaperScissorsStarted();

        /// <summary>
        /// Event when both player set their choice and got the result.
        /// </summary>
        /// <returns></returns>
        RPSResult RPSExecuted();

        /// <summary>
        /// Event when the winner player has to choice who has to start
        /// </summary>
        void ChooseFirstPlayerToPlay();

        /// <summary>
        /// Event when the game start
        /// </summary>
        /// <returns></returns>
        Guid GameStarted();

        /// <summary>
        /// Event when the board is updated
        /// </summary>
        /// <returns></returns>
        Game BoardUpdated();

        /// <summary>
        /// When the server can't process an action from the user
        /// it will throw this error with a error code message.
        /// </summary>
        /// <returns></returns>
        UserAlertMessage UserAlertMessage();

        /// <summary>
        /// When an action is made from any user it will send message code with args.
        /// </summary>
        /// <returns></returns>
        UserGameMessage UserGameMessage();

        /// <summary>
        /// When the user ask for attack, this list will return ids for each attackable character from the opponent.
        /// </summary>
        /// <returns></returns>
        AttackableResult AttackableCards();

        /// <summary>
        /// <para>When the opponent need to react to an action.</para>
        /// </summary>
        /// <returns>True if you need to wait, false to stop waiting.</returns>
        bool WaitOpponent();

        /// <summary>
        /// <para>When an event has trigger and a user has to respond to this event.</para>
        /// <para>Can be use for blocker, counter, opponent turn, ...</para>
        /// <para>Need to call <see cref="IGameHub.ResolveAction"/> to resolve.</para>
        /// </summary>
        /// <returns></returns>
        UserResolver AskUserAction();

        /// <summary>
        /// <para>When the game is finished and there is a winner</para>
        /// </summary>
        /// <returns></returns>
        Guid GameFinished();
    }
}
