using System.Collections.Concurrent;
using System.Net.Sockets;

namespace ChatServer.Services;

public static class ClientManager
{
    // מילון בטוח לשימוש מקבילי (Thread-safe) שממפה בין UserId ל-NetworkStream שלו
    private static readonly ConcurrentDictionary<int, NetworkStream> ConnectedClients = new();

    // רישום משתמש שהתחבר
    public static void RegisterClient(int userId, NetworkStream stream)
    {
        ConnectedClients[userId] = stream;
    }

    // הסרת משתמש שהתנתק
    public static void UnregisterClient(int userId)
    {
        ConnectedClients.TryRemove(userId, out _);
    }

    // שליפת ה-Stream של משתמש לפי ID (אם הוא מחובר)
    public static NetworkStream? GetClientStream(int userId)
    {
        ConnectedClients.TryGetValue(userId, out var stream);
        return stream;
    }
}