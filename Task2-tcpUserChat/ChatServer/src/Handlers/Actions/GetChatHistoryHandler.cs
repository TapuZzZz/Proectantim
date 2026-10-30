using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;
using ChatServer.Services;

namespace ChatServer.Handlers.Actions;

public static class GetChatHistoryHandler
{
    public static async Task HandleAsync(JsonElement payload, NetworkStream stream, UserSession session)
    {
        var response = new NetworkResponse<List<ChatMessageDto>>();

        if (!session.IsAuthenticated || !session.UserId.HasValue)
        {
            response.Success = false;
            response.Message = "Unauthorized: You must be logged in.";
            await SendResponseAsync(stream, response);
            return;
        }

        var data = JsonConverterHelper.Deserialize<GetChatHistoryPayload>(payload.GetRawText());

        if (data == null || data.TargetId <= 0)
        {
            response.Success = false;
            response.Message = "Invalid TargetId.";
            await SendResponseAsync(stream, response);
            return;
        }

        var history = await MessageService.GetChatHistoryAsync(session.UserId.Value, data.TargetId, data.IsGroup);

        response.Success = true;
        response.Message = $"Fetched {history.Count} messages.";
        response.Data = history;

        await SendResponseAsync(stream, response);
    }

    private static async Task SendResponseAsync(NetworkStream stream, NetworkResponse<List<ChatMessageDto>> response)
    {
        string jsonResponse = JsonConverterHelper.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
}