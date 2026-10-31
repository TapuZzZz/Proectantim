namespace ChatServer;

public enum ActionType
{
    Unknown,
    Login,
    Register,
    SendMessage
}

public class Request
{
    public ActionType Action { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class Response
{
    public ActionType Action { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int UserId { get; set; }

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