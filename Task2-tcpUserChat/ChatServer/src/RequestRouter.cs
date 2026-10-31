namespace ChatServer;

public class RequestRouter
{
    private readonly Database _database;
    private readonly OnlineUsers _onlineUsers;

    public RequestRouter(Database database, OnlineUsers onlineUsers)
    {
        _database = database;
        _onlineUsers = onlineUsers;
    }

    public async Task<Response> RouteAsync(Request request, int userId)
    {
        switch (request.Action)
        {
            case ActionType.Login:
                return await HandleLoginAsync(request);

            case ActionType.Register:
                return await HandleRegisterAsync(request);

            case ActionType.SendMessage:
                return await HandleSendMessageAsync(request, userId);

            default:
                return Response.Fail(request.Action, "Unknown action");
        }
    }

    private async Task<Response> HandleLoginAsync(Request request)
    {
        if (IsMissingCredentials(request))
        {
            return Response.Fail(request.Action, "Missing username or password");
        }

        int userId = await _database.FindUserIdAsync(request.Username, request.Password);

        if (userId == -1)
        {
            return Response.Fail(request.Action, "Wrong username or password");
        }

        return CreateLoggedInResponse(request.Action, userId);
    }

    private async Task<Response> HandleRegisterAsync(Request request)
    {
        if (IsMissingCredentials(request))
        {
            return Response.Fail(request.Action, "Missing username or password");
        }

        int userId = await _database.CreateUserAsync(request.Username, request.Password);
        return CreateLoggedInResponse(request.Action, userId);
    }

    private async Task<Response> HandleSendMessageAsync(Request request, int senderId)
    {
        if (string.IsNullOrEmpty(request.Content))
        {
            return Response.Fail(request.Action, "Empty message");
        }

        bool targetExists = await _database.UserExistsAsync(request.TargetId);

        if (!targetExists)
        {
            return Response.Fail(request.Action, "User not found");
        }

        await _database.SaveMessageAsync(senderId, request.TargetId, request.Content);
        await DeliverAsync(senderId, request.TargetId, request.Content);

        return Response.Ok(request.Action, "Message sent");
    }

    // שולח את ההודעה לנמען, אם הוא מחובר עכשיו
    private async Task DeliverAsync(int senderId, int targetId, string content)
    {
        ClientConnection? target = _onlineUsers.Find(targetId);

        if (target == null)
        {
            return;
        }

        Response message = Response.Ok(ActionType.NewMessage, "New message");
        message.SenderId = senderId;
        message.Content = content;

        try
        {
            await target.SendResponseAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Could not deliver to user {targetId}: {ex.Message}");
        }
    }

    private bool IsMissingCredentials(Request request)
    {
        return string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password);
    }

    private Response CreateLoggedInResponse(ActionType action, int userId)
    {
        Response response = Response.Ok(action, "Logged in");
        response.UserId = userId;
        return response;
    }
}