using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;
using ChatServer.Services;

namespace ChatServer.Handlers.Actions;

public static class AddGroupMemberHandler
{
    public static async Task HandleAsync(JsonElement payload, NetworkStream stream, UserSession session)
    {
        var response = new NetworkResponse<string>();

        if (!session.IsAuthenticated || !session.UserId.HasValue)
        {
            response.Success = false;
            response.Message = "Unauthorized: You must be logged in.";
            await SendResponseAsync(stream, response);
            return;
        }

        var data = JsonConverterHelper.Deserialize<AddGroupMemberPayload>(payload.GetRawText());

        if (data == null || data.GroupId <= 0 || data.UserId <= 0)
        {
            response.Success = false;
            response.Message = "Valid GroupId and UserId are required.";
            await SendResponseAsync(stream, response);
            return;
        }

        bool success = await GroupService.AddMemberToGroupAsync(data.GroupId, data.UserId);

        if (success)
        {
            response.Success = true;
            response.Message = $"User ID {data.UserId} added to Group ID {data.GroupId}.";
        }
        else
        {
            response.Success = false;
            response.Message = "Failed to add member to group.";
        }

        await SendResponseAsync(stream, response);
    }

    private static async Task SendResponseAsync(NetworkStream stream, NetworkResponse<string> response)
    {
        string jsonResponse = JsonConverterHelper.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
}