namespace ChatService.DTO;

public record SendMessageDto(string Content, long? ReplyToMessageId);
