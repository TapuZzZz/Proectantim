namespace ChatServer;

public class RequestRouter
{
    private readonly Database _database;

    public RequestRouter(Database database)
    {
        _database = database;
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
                return HandleSendMessage(request, userId);

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

    private Response HandleSendMessage(Request request, int userId)
    {
        // TODO
        return Response.Fail(request.Action, "Not implemented");
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