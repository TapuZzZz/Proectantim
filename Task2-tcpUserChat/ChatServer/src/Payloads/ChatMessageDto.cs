using System;

namespace ChatServer.Payloads;

public class ChatMessageDto
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int TargetId { get; set; }
    public bool IsGroup { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}