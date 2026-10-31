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

    public async Task<List<UserInfo>> GetAllUsersAsync()
    {
        string sql = "SELECT id, username FROM users ORDER BY username";
        List<UserInfo> users = new List<UserInfo>();

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                UserInfo user = new UserInfo();
                user.Id = reader.GetInt32(0);
                user.Username = reader.GetString(1);
                users.Add(user);
            }
        }

        return users;
    }

    public async Task<List<GroupInfo>> GetUserGroupsAsync(int userId)
    {
        string sql = "SELECT g.id, g.name FROM groups g " +
                     "JOIN group_members m ON m.group_id = g.id " +
                     "WHERE m.user_id = @user ORDER BY g.name";
        List<GroupInfo> groups = new List<GroupInfo>();

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("user", userId);

            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    GroupInfo group = new GroupInfo();
                    group.Id = reader.GetInt32(0);
                    group.Name = reader.GetString(1);
                    groups.Add(group);
                }
            }
        }

        return groups;
    }

    public async Task<List<MessageInfo>> GetPrivateHistoryAsync(int userId, int otherId)
    {
        string sql = "SELECT m.sender_id, u.username, m.content, m.created_at FROM messages m " +
                     "JOIN users u ON u.id = m.sender_id " +
                     "WHERE m.is_group = false AND " +
                     "((m.sender_id = @me AND m.target_id = @other) OR (m.sender_id = @other AND m.target_id = @me)) " +
                     "ORDER BY m.created_at, m.id";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("me", userId);
            command.Parameters.AddWithValue("other", otherId);

            return await ReadMessagesAsync(command);
        }
    }

    // כל ההודעות בקבוצה
    public async Task<List<MessageInfo>> GetGroupHistoryAsync(int groupId)
    {
        string sql = "SELECT m.sender_id, u.username, m.content, m.created_at FROM messages m " +
                     "JOIN users u ON u.id = m.sender_id " +
                     "WHERE m.is_group = true AND m.target_id = @group " +
                     "ORDER BY m.created_at, m.id";

        using (NpgsqlConnection connection = await _dataSource.OpenConnectionAsync())
        using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("group", groupId);

            return await ReadMessagesAsync(command);
        }
    }

    // מריץ שאילתה של הודעות
    private async Task<List<MessageInfo>> ReadMessagesAsync(NpgsqlCommand command)
    {
        List<MessageInfo> messages = new List<MessageInfo>();

        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                MessageInfo message = new MessageInfo();
                message.SenderId = reader.GetInt32(0);
                message.SenderName = reader.GetString(1);
                message.Content = reader.GetString(2);
                message.CreatedAt = DateTime.SpecifyKind(reader.GetDateTime(3), DateTimeKind.Utc);
                messages.Add(message);
            }
        }

        return messages;
    }
}