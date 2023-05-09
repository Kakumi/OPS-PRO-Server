namespace OPSProServer.Contracts.Contracts
{
    public class Room
    {
        public Guid Id { get; set; }
        public RoomState State { get; set; }
        public User Creator { get; set; }
        public bool CreatorReady { get; set; }
        public RockPaperScissors CreatorRPS { get; set; }
        public User? Opponent { get; set; }
        public bool OpponentReady { get; set; }
        public RockPaperScissors OpponentRPS { get; set; }
        public DateTime Created { get; set; }
        public bool UsePassword { get; set; }
        public string? Password { get; set; }
        public string? Description { get; set; }
        public Guid? FirstToPlay { get; set; }

        public Room(User user)
        {
            Id = Guid.NewGuid();
            State = RoomState.Created;
            Creator = user;
            Created = DateTime.Now;
            CreatorReady = false;
            OpponentReady = false;
        }

        public Room() { }

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
            return Opponent != null && OpponentReady && Creator != null && CreatorReady;
        }

        public void RemoveOpponent()
        {
            Opponent = null;
            OpponentReady = false;
            OpponentRPS = RockPaperScissors.None;
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

        public Room Clone()
        {
            return new Room()
            {
                Id = this.Id,
                Creator = this.Creator,
                Opponent = this.Opponent,
                Password = this.Password,
                Created = this.Created,
                Description = this.Description,
                CreatorReady = this.CreatorReady,
                OpponentReady = this.OpponentReady,
                UsePassword = this.UsePassword,
            };
        }

        public Guid? GetRockPaperScissorsWinner()
        {
            if (Opponent != null && CreatorRPS != RockPaperScissors.None && OpponentRPS != RockPaperScissors.None)
            {
                // Define the relationships between moves
                int[,] relationships = { { 0, -1, 1 }, { 1, 0, -1 }, { -1, 1, 0 } };

                // Determine winner
                int creatorIndex = (int)CreatorRPS - 1;
                int opponentIndex = (int)OpponentRPS - 1;
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
