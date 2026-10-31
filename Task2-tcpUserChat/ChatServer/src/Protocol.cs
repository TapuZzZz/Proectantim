namespace ChatServer;

public enum ActionType
{
    Unknown,
    Login,
    Register,
    SendMessage,
    NewMessage      // רק מהשרת ללקוח: הודעה נכנסת
}

public class Request
{
    public ActionType Action { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public int TargetId { get; set; }
    public string Content { get; set; } = "";
}

public class Response
{
    public ActionType Action { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int UserId { get; set; }
    public int SenderId { get; set; }
    public string? Content { get; set; }

    public static Response Ok(ActionType action, string message)
    {
        Response response = new Response();
        response.Action = action;
        response.Success = true;
        response.Message = message;
        return response;
    }

    public static Response Fail(ActionType action, string message)
    {
        Response response = new Response();
        response.Action = action;
        response.Success = false;
        response.Message = message;
        return response;
    }
}