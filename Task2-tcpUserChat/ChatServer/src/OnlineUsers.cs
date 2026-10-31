namespace ChatServer;

public class OnlineUsers
{
    private readonly Dictionary<int, ClientConnection> _connections = new Dictionary<int, ClientConnection>();
    private readonly object _lock = new object();

    public void Add(int userId, ClientConnection connection)
    {
        lock (_lock) // בשביל למנוע RACE CONDITION
        {
            _connections[userId] = connection;
        }
    }

    // מוחק רק אם זה אותו חיבור, ולא חיבור חדש יותר של אותו משתמש
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

    // מחזיר את החיבור של המשתמש, או null אם הוא לא מחובר
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