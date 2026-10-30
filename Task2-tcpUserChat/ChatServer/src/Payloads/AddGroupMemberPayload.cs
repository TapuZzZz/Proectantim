namespace ChatServer.Payloads;

public class AddGroupMemberPayload
{
    public int GroupId { get; set; }
    public int UserId { get; set; }
}