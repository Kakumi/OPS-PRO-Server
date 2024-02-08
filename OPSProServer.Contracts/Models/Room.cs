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
            return Opponent == null && Creator.Id != user.Id && (string.IsNullOrEmpty(Password) || Password == password);
        }

        public Game StartGame(Guid userToStart)
        {
            var creatorInfo = new PlayerGameInformation(Creator, Creator.Deck!,new OpponentPhase(userToStart == Creator.Id));
            var opponentInfo = new PlayerGameInformation(Opponent, Opponent.Deck!, new OpponentPhase(userToStart == Opponent.Id));
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

            return new RPSResult(Creator.Id, Creator.RPSChoice, Opponent!.Id, Opponent.RPSChoice, winnerId);
        }
    }
}
