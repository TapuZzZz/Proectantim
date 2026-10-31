using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChatServer;

public class ClientConnection
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly RequestRouter _router;
    private readonly OnlineUsers _onlineUsers;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _endPoint;

    private int _userId = -1;   // -1 = עוד לא התחבר

    public ClientConnection(TcpClient client, RequestRouter router, OnlineUsers onlineUsers)
    {
        _client = client;
        _router = router;
        _onlineUsers = onlineUsers;
        _stream = client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _jsonOptions = CreateJsonOptions();
        _endPoint = $"{client.Client.RemoteEndPoint}";
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"[+] Client connected: {_endPoint}");

        try
        {
            await ReceiveLoopAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error with client {_endPoint}: {ex.Message}");
        }
        finally
        {
            Close();
        }
    }

    // קורא שורות מהלקוח עד שהוא מתנתק
    private async Task ReceiveLoopAsync()
    {
        while (true)
        {
            string? line = await _reader.ReadLineAsync();

            if (line == null)
            {
                return;
            }

            Response response = await BuildResponseAsync(line);
            await SendResponseAsync(response);
        }
    }

    // מחליט מה התשובה לשורה שהתקבלה
    private async Task<Response> BuildResponseAsync(string line)
    {
        Request? request = ParseRequest(line);

        if (request == null)
        {
            return Response.Fail(ActionType.Unknown, "Invalid request");
        }

        bool isAuthAction = IsAuthAction(request.Action);

        if (!IsLoggedIn() && !isAuthAction)
        {
            return Response.Fail(request.Action, "Login required");
        }

        if (IsLoggedIn() && isAuthAction)
        {
            return Response.Fail(request.Action, "Already logged in");
        }

        Response response = await _router.RouteAsync(request, _userId);
        SaveLoginIfSucceeded(response);
        return response;
    }

    // מוסיף לקוח מחובר לרשימה של הלקוחות ושומר את האיידי שלו
        private void SaveLoginIfSucceeded(Response response)
    {
        if (response.Success && IsAuthAction(response.Action))
        {
            _userId = response.UserId;
            _onlineUsers.Add(_userId, this);
            Console.WriteLine($"[+] User {_userId} logged in from {_endPoint} (online: {_onlineUsers.Count()})");
        }
    }

    private bool IsLoggedIn()
    {
        return _userId != -1;
    }

    private bool IsAuthAction(ActionType action)
    {
        return action == ActionType.Login || action == ActionType.Register;
    }

    private Request? ParseRequest(string json)
    {
        try
        {
            Request? request = JsonSerializer.Deserialize<Request>(json, _jsonOptions);

            if (request == null || request.Action == ActionType.Unknown)
            {
                return null;
            }

            return request;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private async Task SendResponseAsync(Response response)
    {
        string json = JsonSerializer.Serialize(response, _jsonOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json + "\n");
        await _stream.WriteAsync(bytes, 0, bytes.Length);
    }

    private JsonSerializerOptions CreateJsonOptions()
    {
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.PropertyNameCaseInsensitive = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private void Close()
    {
        if (IsLoggedIn())
        {
            _onlineUsers.Remove(_userId, this);
        }

        _reader.Dispose();
        _client.Dispose();
        Console.WriteLine($"[-] Client disconnected: {_endPoint} (online: {_onlineUsers.Count()})");
    }
}