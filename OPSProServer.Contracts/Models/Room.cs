using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPSProServer.Contracts.Models
{
    public class Room : SecureRoom
    {
        public string? Password { get; private set; }
        public Game? Game { get; private set; }

        public Room(User user, string? description = null, string? password = null) : base(user, !string.IsNullOrEmpty(password), description)
        {
            Password = password;
        }

        public bool IsJoinable(User user, string? password)
        {
            return Opponent == null && Creator.Id != user.Id && Password == password;
        }

        public Game StartGame(Guid userToStart)
        {
            var creatorInfo = new PlayerGameInformation(Creator.Id, Creator.Username, Creator.Deck!, userToStart == Creator.Id ? new RefreshPhase() : new OpponentPhase());
            var opponentInfo = new PlayerGameInformation(Opponent!.Id, Opponent.Username, Opponent.Deck!, userToStart == Opponent.Id ? new RefreshPhase() : new OpponentPhase());
            State = RoomState.InGame;
            Game = new Game(userToStart, creatorInfo, opponentInfo);
            return Game;
        }

        public Guid? GetRPSWinnerId()
        {
            if (Creator.RPSChoice != RPSChoice.None && Opponent != null && Opponent.RPSChoice != RPSChoice.None)
            {
                // Define the relationships between moves
                int[,] relationships = { { 0, -1, 1 }, { 1, 0, -1 }, { -1, 1, 0 } };

                // Determine winner
                int creatorIndex = (int)Creator.RPSChoice - 1;
                int opponentIndex = (int)Opponent.RPSChoice - 1;
                int result = relationships[creatorIndex, opponentIndex];

                // Return winner id
                if (result == 1)
                {
                    return Creator.Id;
                }
                else if (result == -1)
                {
                    return Opponent.Id;
                }
            }

            return null;
        }

        public RPSResult GetRPSWinner()
        {
            var dic = new Dictionary<Guid, RPSChoice>();
            dic.Add(Creator.Id, Creator.RPSChoice);
            dic.Add(Opponent!.Id, Opponent.RPSChoice);

            var winnerId = GetRPSWinnerId();

            return new RPSResult(winnerId, dic);
        }
    }
}
