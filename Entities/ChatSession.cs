namespace DripCube.Entities
{
    public class ChatSession
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        public ChatStatus Status { get; set; } = ChatStatus.New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatMessage> Messages { get; set; } = new();
    }
}