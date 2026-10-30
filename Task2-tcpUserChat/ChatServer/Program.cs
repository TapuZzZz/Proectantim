using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Npgsql;

using ChatServer.Handlers;
using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;

namespace ChatServer.Program;

class Program
{
    public const int ServerPort = 8080; // ** magiC Numbers 
    private static readonly TcpListener _listener = new TcpListener(IPAddress.Any, ServerPort);
    public static async Task Main()
    {
        _listener.Start();
        Console.WriteLine($"Server started on port {ServerPort}...");

        while(true)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();

            _ = HandleClientAsync(client);
        }

    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        var endPoint = client.Client.RemoteEndPoint;
        Console.WriteLine($"[+] Client connected: {endPoint}");

        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                while (true)
                {
                    var request = await ReadRequestAsync(stream);

                    if (request == null)
                    {
                        break; 
                    }

                    Console.WriteLine($"[+] Action received: {request.Action}");
                    await RequestHandler.HandleRequestAsync(request, stream);
                }
            }
        }
        catch (SocketException)
        {
            Console.WriteLine($"[!] Client {endPoint} disconnected abruptly (Socket error).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error handling client {endPoint}: {ex.Message}");
        }
        finally
        {
            Console.WriteLine($"[-] Cleaned up resources for client: {endPoint}");
        }
    }

    private static async Task<NetworkRequest<JsonElement>?> ReadRequestAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        if (bytesRead == 0)
        {
            return null;
        }

        string rawJson = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
        return JsonConverterHelper.Deserialize<NetworkRequest<JsonElement>>(rawJson);
    }
    
}