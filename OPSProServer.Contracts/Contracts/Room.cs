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
    }
}
