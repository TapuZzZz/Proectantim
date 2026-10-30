namespace ChatServer.Protocol;

public class UserSession
{
    public int? UserId { get; set; }
    public string? Username { get; set; }

    public bool IsAuthenticated => UserId.HasValue;
}