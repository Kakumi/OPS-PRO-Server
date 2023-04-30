namespace OPSProServer.Contracts.Contracts
{
    public class Room
    {
        public Guid Id { get; set; }
        public User Creator { get; set; }
        public bool CreatorReady { get; set; }
        public User? Opponent { get; set; }
        public bool OpponentReady { get; set; }
        public DateTime Created { get; set; }
        public bool UsePassword { get; set; }
        public string? Password { get; set; }
        public string? Description { get; set; }

        public Room(User user)
        {
            Id = Guid.NewGuid();
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
