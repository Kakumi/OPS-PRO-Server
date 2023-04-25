namespace OPS_Pro_Server.Models
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
    }
}
