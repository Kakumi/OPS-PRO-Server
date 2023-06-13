namespace OPSProServer.Contracts.Contracts
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
        public RPSGame? RPSGame { get; private set; }
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

        public void SetOpponent(User opponent)
        {
            if (opponent != null)
            {
                Opponent = new UserRoom(opponent);
                RPSGame = new RPSGame(Creator.Id, Opponent.Id);
            }
        }

        public void RemoveOpponent()
        {
            Opponent = null;
            RPSGame = null;
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

        public void StartGame()
        {
            if (RPSGame != null)
            {
                var winner = RPSGame.GetWinner();
                if (winner != null && CanStart())
                {
                    var creatorInfo = new PlayerGameInformation(Creator.Id, Creator.Deck!);
                    var opponentInfo = new PlayerGameInformation(Creator.Id, Opponent.Deck!);
                    Game = new Game(winner.Value, creatorInfo, opponentInfo);
                }
            }
        }

        public Room Clone()
        {
            return new Room(Creator, Description, Password)
            {
                Id = this.Id,
                Creator = this.Creator,
                Opponent = this.Opponent,
                Password = this.Password,
                Created = this.Created,
                Description = this.Description,
                UsePassword = this.UsePassword,
                RPSGame = this.RPSGame,
                Game = this.Game
            };
        }
    }
}
