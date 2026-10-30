namespace ChatServer.Payloads;

public class SendMessagePayload
{
    // TargetId יכול להיות ID של משתמש מקבל (בפרטי) או ID של קבוצה
    public int TargetId { get; set; } 
    
    // true = שליחה לקבוצה, false = הודעה פרטית למשתמש
    public bool IsGroup { get; set; } 

    // תוכן ההודעה
    public string Content { get; set; } = string.Empty;
}