namespace ChatService.Infrastructure.Models;

public class ChatMessage
{
    public long      Id               { get; set; }
    public long      GroupId          { get; set; }
    public long      SenderUserId     { get; set; }
    public string    Content          { get; set; } = string.Empty;
    public long?     ReplyToMessageId { get; set; }
    public DateTime  CreatedAt        { get; set; }
    public DateTime? EditedAt         { get; set; }
    public DateTime? DeletedAt        { get; set; }
}
