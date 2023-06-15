using OPSProServer.Models;

namespace OPSProServer.Hubs
{
    public interface IRoomHub
    {
        Task<bool> CreateRoom(Guid userId, string? password, string? description);
        Task<List<SecureRoom>> GetRooms();
        Task<SecureRoom> GetRoom(Guid userId);
        Task<bool> JoinRoom(Guid userId, Guid roomId, string? password);
        Task<bool> LeaveRoom(Guid userId);
        Task<bool> SetReady(Guid userId, DeckInfo? deckInfo);
        Task<bool> Exclude(Guid userId, Guid opponentId, Guid roomId);
    }
}
