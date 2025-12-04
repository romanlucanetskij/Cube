using System.ComponentModel.DataAnnotations;

namespace DripCube.Dtos
{

    public class SendMessageDto
    {
        public Guid SenderId { get; set; }
        public string Text { get; set; } = string.Empty;


        public Guid? SessionId { get; set; }
    }


    public class ChatPreviewDto
    {
        public Guid SessionId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }


    public class MessageDto
    {
        public string Sender { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}