namespace ChatService.DTO;

public record MessageResponse(
    long      Id,
    long      GroupId,
    long      SenderUserId,
    string    Content,
    long?     ReplyToMessageId,
    System.DateTime CreatedAt,
    System.DateTime? EditedAt,
    bool      IsDeleted
);
