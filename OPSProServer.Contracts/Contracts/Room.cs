﻿namespace OPSProServer.Contracts.Contracts
{
    public class Room
    {
        public Guid Id { get; private set; }
        public RoomState State { get; set; }
        public UserRoom Creator { get; private set; }
        public UserRoom? Opponent { get; private set; }
        public DateTime Created { get; private set; }
        public bool UsePassword { get; private set; }
        public string? Password { get; private set; }
        public string? Description { get; private set; }
        public Game? Game { get; private set; }

        public Room(User user, string? description = null, string? password = null)
        {
            Id = Guid.NewGuid();
            State = RoomState.Created;
            Creator = new UserRoom(user);
            Created = DateTime.Now;
            Description = description;
            UsePassword = !string.IsNullOrEmpty(password);
            Password = password;
        }

        public bool IsJoinable(User user, string? password)
        {
            return Opponent == null && Creator.Id != user.Id && Password == password;
        }

        public bool IsInside(User user)
        {
            return Creator.Id == user.Id || Opponent?.Id == user.Id;
        }

        public bool CanStart()
        {
            return Opponent != null && Opponent.Ready && Creator != null && Creator.Ready;
        }

        public void SetOpponent(User? opponent)
        {
            if (opponent != null)
            {
                Opponent = new UserRoom(opponent);
            } else
            {
                Opponent = null;
            }
        }

        public User? GetOpponent(Guid userId)
        {
            if (userId == Creator.Id)
            {
                return Opponent;
            }

            return Creator;
        }

        public User? GetOpponent(User user)
        {
            if (user.Id == Creator.Id)
            {
                return Opponent;
            }

            return Creator;
        }

        public void StartGame(Guid userToStart)
        {
            var creatorInfo = new PlayerGameInformation(Creator.Id, Creator.Deck!);
            var opponentInfo = new PlayerGameInformation(Creator.Id, Opponent!.Deck!);
            Game = new Game(userToStart, creatorInfo, opponentInfo);
        }

        public Guid? GetRPSWinner()
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
    }
}
