using System;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer.Protocol;
using ChatServer.Handlers.Actions;

namespace ChatServer.Handlers;

public static class RequestHandler
{
    public static async Task HandleRequestAsync(NetworkRequest<JsonElement> request, NetworkStream stream, UserSession session)
    {
        switch (request.Action)
        {
            case ActionType.Register:
                await RegisterHandler.HandleAsync(request.Payload, stream);
                break;

            case ActionType.Login:
                await LoginHandler.HandleAsync(request.Payload, stream, session);
                break;

            case ActionType.SendMessage:
                await SendMessageHandler.HandleAsync(request.Payload, stream, session);
                break;

            case ActionType.GetChatHistory:
                await GetChatHistoryHandler.HandleAsync(request.Payload, stream, session);
                break;

            case ActionType.CreateGroup:
                await CreateGroupHandler.HandleAsync(request.Payload, stream, session);
                break;

            case ActionType.AddGroupMember:
                await AddGroupMemberHandler.HandleAsync(request.Payload, stream, session);
                break;

            default:
                Console.WriteLine($"[!] Action not supported yet: {request.Action}");
                break;
        }
    }
}