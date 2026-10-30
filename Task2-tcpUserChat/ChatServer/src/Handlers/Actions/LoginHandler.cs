using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;
using ChatServer.Services;

namespace ChatServer.Handlers.Actions;

public static class LoginHandler
{
    // הוסיפנו פרמטר userSession כדי לשמור את המצב בזיכרון
    public static async Task HandleAsync(JsonElement payload, NetworkStream stream, UserSession session)
    {
        var loginData = JsonConverterHelper.Deserialize<LoginPayload>(payload.GetRawText());
        var response = new NetworkResponse<string>();

        if (loginData == null || string.IsNullOrWhiteSpace(loginData.Username) || string.IsNullOrWhiteSpace(loginData.Password))
        {
            response.Success = false;
            response.Message = "Username and password are required.";
        }
        else
        {
            // מקבלים int? במקום bool
            int? userId = await AuthService.ValidateUserCredentialsAsync(loginData.Username, loginData.Password);

            if (userId.HasValue)
            {
                session.UserId = userId.Value;
                session.Username = loginData.Username;

                ClientManager.RegisterClient(userId.Value, stream);

                response.Success = true;
                response.Message = $"Login successful! Welcome user ID {userId.Value}.";
                response.Data = loginData.Username;
            }
            else
            {
                response.Success = false;
                response.Message = "Invalid username or password.";
            }
        }

        string jsonResponse = JsonConverterHelper.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
}