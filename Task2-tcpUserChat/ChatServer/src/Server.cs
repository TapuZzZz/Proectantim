using System.Net;
using System.Net.Sockets;

namespace ChatServer;

public class Server
{
    private readonly int _port;
    private readonly TcpListener _listener;
    private readonly RequestRouter _router;

    public Server(int port, Database database)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, port);
        _router = new RequestRouter(database);
    }

    public async Task RunAsync()
    {
        _listener.Start();
        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
            ClientConnection connection = new ClientConnection(tcpClient, _router);
            _ = connection.RunAsync();
        }
    }
}