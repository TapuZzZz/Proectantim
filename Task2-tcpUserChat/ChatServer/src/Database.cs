using Npgsql;

namespace ChatServer;

public class Database
{
    private readonly NpgsqlDataSource _dataSource;

    public Database(string connectionString)
    {
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public async Task TestConnectionAsync()
    {
        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand("SELECT 1", connection))
        {
            await command.ExecuteScalarAsync();
        }
    }

    // מחזיר את ה-id של המשתמש, או -1 אם השם או הסיסמה שגויים
    public async Task<int> FindUserIdAsync(string username, string password)
    {
        string sql = "SELECT id, password_hash FROM users WHERE username = @username";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("username", username);

            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    string hash = reader.GetString(1);

                    if (BCrypt.Net.BCrypt.Verify(password, hash))
                    {
                        return id;
                    }
                }
            }
        }

        return -1;
    }

    // יוצר משתמש חדש ומחזיר את ה-id שלו
    public async Task<int> CreateUserAsync(string username, string password)
    {
        string hash = BCrypt.Net.BCrypt.HashPassword(password);
        string sql = "INSERT INTO users (username, password_hash) VALUES (@username, @hash) RETURNING id";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("username", username);
            command.Parameters.AddWithValue("hash", hash);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        string sql = "SELECT COUNT(*) FROM users WHERE id = @id";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("id", userId);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }

    public async Task SaveMessageAsync(int senderId, int targetId, bool isGroup, string content)
    {
        string sql = "INSERT INTO messages (sender_id, target_id, is_group, content) VALUES (@sender, @target, @isGroup, @content)";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("sender", senderId);
            command.Parameters.AddWithValue("target", targetId);
            command.Parameters.AddWithValue("isGroup", isGroup);
            command.Parameters.AddWithValue("content", content);

            await command.ExecuteNonQueryAsync();
        }
    }

    // יוצר קבוצה חדשה ומחזיר את ה-id שלה
    public async Task<int> CreateGroupAsync(string name)
    {
        string sql = "INSERT INTO groups (name) VALUES (@name) RETURNING id";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("name", name);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }

    // מוסיף משתמש לקבוצה
    public async Task AddGroupMemberAsync(int groupId, int userId)
    {
        string sql = "INSERT INTO group_members (group_id, user_id) VALUES (@group, @user)";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("group", groupId);
            command.Parameters.AddWithValue("user", userId);

            await command.ExecuteNonQueryAsync();
        }
    }

    // האם המשתמש חבר בקבוצה
    public async Task<bool> IsGroupMemberAsync(int groupId, int userId)
    {
        string sql = "SELECT COUNT(*) FROM group_members WHERE group_id = @group AND user_id = @user";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("group", groupId);
            command.Parameters.AddWithValue("user", userId);

            object? result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
    }

    // מחזיר את ה-id של כל חברי הקבוצה
    public async Task<List<int>> GetGroupMemberIdsAsync(int groupId)
    {
        string sql = "SELECT user_id FROM group_members WHERE group_id = @group";
        List<int> memberIds = new List<int>();

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("group", groupId);

            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    memberIds.Add(reader.GetInt32(0));
                }
            }
        }

        return memberIds;
    }

    // מחזיר את שם המשתמש לפי ה-id שלו
    public async Task<string> GetUsernameAsync(int userId)
    {
        string sql = "SELECT username FROM users WHERE id = @id";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("id", userId);

            object? result = await command.ExecuteScalarAsync();

            if (result == null)
            {
                return "";
            }

            return (string)result;
        }
    }
}