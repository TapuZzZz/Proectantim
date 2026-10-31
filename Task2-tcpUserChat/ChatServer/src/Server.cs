using System.Net;
using System.Net.Sockets;

namespace ChatServer;

public class Server
{
    private readonly int _port;
    private readonly TcpListener _listener;
    private readonly RequestRouter _router;
    private readonly OnlineUsers _onlineUsers;

    public Server(int port, Database database)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, port);
        _onlineUsers = new OnlineUsers();
        _router = new RequestRouter(database, _onlineUsers);
    }

    public async Task RunAsync()
    {
        _listener.Start();
        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
            ClientConnection connection = new ClientConnection(tcpClient, _router, _onlineUsers);
            _ = connection.RunAsync();
        }
    }
}