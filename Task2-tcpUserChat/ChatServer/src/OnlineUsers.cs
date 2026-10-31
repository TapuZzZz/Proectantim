namespace ChatServer;

public class OnlineUsers
{
    private readonly Dictionary<int, ClientConnection> _connections = new Dictionary<int, ClientConnection>();
    private readonly object _lock = new object();  // בשביל למנוע RACE CONDITION

    public void Add(int userId, ClientConnection connection)
    {
        lock (_lock)
        {
            _connections[userId] = connection;
        }
    }

    public void Remove(int userId, ClientConnection connection)
    {
        lock (_lock)
        {
            if (_connections.ContainsKey(userId) && _connections[userId] == connection)
            {
                _connections.Remove(userId);
            }
        }
    }

    public ClientConnection? Find(int userId)
    {
        lock (_lock)
        {
            if (_connections.ContainsKey(userId))
            {
                return _connections[userId];
            }

            return null;
        }
    }

    public int Count()
    {
        lock (_lock)
        {
            return _connections.Count;
        }
    }
}