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

            case ActionType.CreateGroup:
                return await HandleCreateGroupAsync(request, userId);

            case ActionType.AddGroupMember:
                return await HandleAddGroupMemberAsync(request, userId);

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

        if (request.IsGroup)
        {
            return await SendGroupMessageAsync(request, senderId);
        }

        return await SendPrivateMessageAsync(request, senderId);
    }

    private async Task<Response> SendPrivateMessageAsync(Request request, int senderId)
    {
        bool targetExists = await _database.UserExistsAsync(request.TargetId);

        if (!targetExists)
        {
            return Response.Fail(request.Action, "User not found");
        }

        await _database.SaveMessageAsync(senderId, request.TargetId, false, request.Content);

        Response message = await CreateNewMessageAsync(senderId, request.Content, 0);
        await DeliverAsync(request.TargetId, message);

        return Response.Ok(request.Action, "Message sent");
    }

    private async Task<Response> SendGroupMessageAsync(Request request, int senderId)
    {
        int groupId = request.TargetId;
        bool isMember = await _database.IsGroupMemberAsync(groupId, senderId);

        if (!isMember)
        {
            return Response.Fail(request.Action, "You are not in this group");
        }

        await _database.SaveMessageAsync(senderId, groupId, true, request.Content);

        Response message = await CreateNewMessageAsync(senderId, request.Content, groupId);
        List<int> memberIds = await _database.GetGroupMemberIdsAsync(groupId);

        foreach (int memberId in memberIds)
        {
            if (memberId != senderId)
            {
                await DeliverAsync(memberId, message);
            }
        }

        return Response.Ok(request.Action, "Message sent");
    }

    private async Task<Response> CreateNewMessageAsync(int senderId, string content, int groupId)
    {
        string senderName = await _database.GetUsernameAsync(senderId);

        Response message = Response.Ok(ActionType.NewMessage, "New message");
        message.SenderId = senderId;
        message.SenderName = senderName;
        message.Content = content;
        message.GroupId = groupId;
        return message;
    }

    private async Task<Response> HandleCreateGroupAsync(Request request, int userId)
    {
        if (string.IsNullOrEmpty(request.GroupName))
        {
            return Response.Fail(request.Action, "Missing group name");
        }

        int groupId = await _database.CreateGroupAsync(request.GroupName);
        await _database.AddGroupMemberAsync(groupId, userId);

        Response response = Response.Ok(request.Action, "Group created");
        response.GroupId = groupId;
        return response;
    }

    private async Task<Response> HandleAddGroupMemberAsync(Request request, int userId)
    {
        bool isMember = await _database.IsGroupMemberAsync(request.GroupId, userId);

        if (!isMember)
        {
            return Response.Fail(request.Action, "You are not in this group");
        }

        bool targetExists = await _database.UserExistsAsync(request.TargetId);

        if (!targetExists)
        {
            return Response.Fail(request.Action, "User not found");
        }

        bool alreadyMember = await _database.IsGroupMemberAsync(request.GroupId, request.TargetId);

        if (alreadyMember)
        {
            return Response.Fail(request.Action, "User already in group");
        }

        await _database.AddGroupMemberAsync(request.GroupId, request.TargetId);

        Response response = Response.Ok(request.Action, "Member added");
        response.GroupId = request.GroupId;
        return response;
    }

    // שולח את ההודעה לנמען, אם הוא מחובר עכשיו
    private async Task DeliverAsync(int targetId, Response message)
    {
        ClientConnection? target = _onlineUsers.Find(targetId);

        if (target == null)
        {
            return;
        }

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