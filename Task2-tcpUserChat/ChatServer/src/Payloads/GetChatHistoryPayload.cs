namespace ChatServer.Payloads;

public class GetChatHistoryPayload
{
    public int TargetId { get; set; } // ID של המשתמש או הקבוצה שרוצים את ההיסטוריה מולו
    public bool IsGroup { get; set; } // true = קבוצה, false = הודעות פרטיות
}