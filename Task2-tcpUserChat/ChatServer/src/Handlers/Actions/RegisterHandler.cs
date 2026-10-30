using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;
using ChatServer.Services; // משתמש בשירות ה-DB בדיוק כמו שתיכננת!

namespace ChatServer.Handlers.Actions;

public static class RegisterHandler
{
    public static async Task HandleAsync(JsonElement payload, NetworkStream stream)
    {
        var registerData = JsonConverterHelper.Deserialize<RegisterPayload>(payload.GetRawText());
        var response = new NetworkResponse<string>();

        if (registerData == null || string.IsNullOrWhiteSpace(registerData.Username) || string.IsNullOrWhiteSpace(registerData.Password))
        {
            response.Success = false;
            response.Message = "Username and password are required for registration.";
        }
        else
        {
            // קורא ל-AuthService במקום להחזיק SQL בתוך ה-Handler
            bool isCreated = await AuthService.CreateUserAsync(registerData.Username, registerData.Password);

            if (isCreated)
            {
                response.Success = true;
                response.Message = "User registered successfully!";
                response.Data = registerData.Username;
            }
            else
            {
                response.Success = false;
                response.Message = "Username already exists or registration failed.";
            }
        }

        string jsonResponse = JsonConverterHelper.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
}