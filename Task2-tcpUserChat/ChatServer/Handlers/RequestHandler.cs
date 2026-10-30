using System;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using Npgsql;
using BCrypt.Net;

using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;

namespace ChatServer.Handlers;

public static class RequestHandler
{
    private const string DbConnectionString = "Host=ep-lively-darkness-bavzh4f1.c-8.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_Gs8hbWM7tByj;SSL Mode=Require;";

    public static async Task HandleRequestAsync(NetworkRequest<JsonElement> request, NetworkStream stream)
    {
        switch (request.Action)
        {
            case ActionType.Register:
                await HandleRegisterAsync(request.Payload, stream);
                break;
            case ActionType.Login:
                await HandleLoginAsync(request.Payload, stream);
                break;

            default:
                Console.WriteLine($"[!] Action not supported yet: {request.Action}");
                break;
        }
    }

    private static async Task HandleRegisterAsync(JsonElement payload, NetworkStream stream)
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
            bool isCreated = await CreateUserAsync(registerData.Username, registerData.Password);

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

    private static async Task<bool> CreateUserAsync(string username, string password)
    {
        try
        {
            using var conn = new NpgsqlConnection(DbConnectionString);
            await conn.OpenAsync();

            string sql = "INSERT INTO users (username, password_hash) VALUES (@u, @p)";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("u", username);
            cmd.Parameters.AddWithValue("p", passwordHash);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // קוד המסמן שגיאה שיש אחד כזה כבר
        {
            return false;
        }
    }

    private static async Task HandleLoginAsync(JsonElement payload, NetworkStream stream)
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
            bool isAuthenticated = await ValidateUserCredentialsAsync(loginData.Username, loginData.Password);

            if (isAuthenticated)
            {
                response.Success = true;
                response.Message = "Login successful!";
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

    // פונקציית עזר קטנה לאימות מול בסיס הנתונים
    private static async Task<bool> ValidateUserCredentialsAsync(string username, string password)
    {
        using var conn = new NpgsqlConnection(DbConnectionString);
        await conn.OpenAsync();

        string sql = "SELECT password_hash FROM users WHERE username = @u";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("u", username);

        var savedHash = await cmd.ExecuteScalarAsync() as string;

        if (string.IsNullOrEmpty(savedHash))
        {
            return false;
        }

        return BCrypt.Net.BCrypt.Verify(password, savedHash);
    }
}