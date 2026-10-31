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
}