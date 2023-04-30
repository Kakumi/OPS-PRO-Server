namespace OPSProServer.Contracts.Contracts
{
    public class Room
    {
        public Guid Id { get; set; }
        public User Creator { get; set; }
        public User? Opponent { get; set; }
        public DateTime Created { get; set; }
        public string? Password { get; set; }
        public string? Description { get; set; }

        public Room(User user)
        {
            Id = Guid.NewGuid();
            Creator = user;
            Created = DateTime.Now;
        }

        public bool IsJoinable(User user, string? password)
        {
            return Opponent == null && Creator.Id != user.Id && Password == password;
        }
    }
}
