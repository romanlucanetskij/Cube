namespace DripCube.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }

        public Guid ChatSessionId { get; set; }
        public ChatSession? ChatSession { get; set; }

        public SenderRole Sender { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}