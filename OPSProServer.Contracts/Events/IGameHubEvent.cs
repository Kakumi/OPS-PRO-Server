using OPSProServer.Contracts.Models;
using System;

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
    }
}
