using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;
using ChatServer.Services;

namespace ChatServer.Handlers.Actions;

public static class CreateGroupHandler
{
    public static async Task HandleAsync(JsonElement payload, NetworkStream stream, UserSession session)
    {
        var response = new NetworkResponse<int>();

        if (!session.IsAuthenticated || !session.UserId.HasValue)
        {
            response.Success = false;
            response.Message = "Unauthorized: You must be logged in to create a group.";
            await SendResponseAsync(stream, response);
            return;
        }

        var data = JsonConverterHelper.Deserialize<CreateGroupPayload>(payload.GetRawText());

        if (data == null || string.IsNullOrWhiteSpace(data.GroupName))
        {
            response.Success = false;
            response.Message = "GroupName is required.";
            await SendResponseAsync(stream, response);
            return;
        }

        int? groupId = await GroupService.CreateGroupAsync(data.GroupName, session.UserId.Value);

        if (groupId.HasValue)
        {
            response.Success = true;
            response.Message = $"Group '{data.GroupName}' created successfully.";
            response.Data = groupId.Value;
        }
        else
        {
            response.Success = false;
            response.Message = "Failed to create group.";
        }

        await SendResponseAsync(stream, response);
    }

    private static async Task SendResponseAsync(NetworkStream stream, NetworkResponse<int> response)
    {
        string jsonResponse = JsonConverterHelper.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
}