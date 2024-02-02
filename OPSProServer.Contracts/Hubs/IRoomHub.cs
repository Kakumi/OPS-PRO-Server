using OPSProServer.Contracts.Models;
using OPSProServer.Contracts.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OPSProServer.Contracts.Hubs
{
    public interface IRoomHub
    {
        /// <summary>
        /// <para>Create a new room.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="password"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        Task<bool> CreateRoom(string? password, string? description);

        /// <summary>
        /// Get all rooms.
        /// </summary>
        /// <returns></returns>
        Task<List<SecureRoom>> GetRooms();

        /// <summary>
        /// Get a room for a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<SecureRoom> GetRoom();

        /// <summary>
        /// <para>Join a room.</para>
        /// <para>When joined successful, will fire <see cref="IRoomHubEvent.RoomUpdated"/>.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roomId"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> JoinRoom(Guid roomId, string? password);

        /// <summary>
        /// <para>Leave a room.</para>
        /// <para>When left successful (opponent), will fire <see cref="IRoomHubEvent.RoomUpdated"/>.</para>
        /// <para>When left successful (creator), will fire <see cref="IRoomHubEvent.RoomDeleted"/>.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> LeaveRoom();

        /// <summary>
        /// <para>Set the user as ready for his current room.</para>
        /// <para>Will fire <see cref="IRoomHubEvent.RoomUpdated"/>.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="deckInfo"></param>
        /// <returns></returns>
        Task<bool> SetReady(string name, List<string> cardsId);//DeckInfo? deckInfo);

        /// <summary>
        /// <para>Exclude a user from the room.</para>
        /// <para>Will fire <see cref="IRoomHubEvent.RoomUpdated"/>.</para>
        /// <para>Will fire <see cref="IRoomHubEvent.RoomExcluded"/> to the excluded user.</para>
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="opponentId"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        Task<bool> Exclude(Guid opponentId, Guid roomId);
    }
}
