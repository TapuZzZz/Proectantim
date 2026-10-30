using Npgsql;
using BCrypt.Net;

namespace ChatServer.Services;

public static class AuthService
{
    private const string DbConnectionString = "Host=ep-lively-darkness-bavzh4f1.c-8.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_Gs8hbWM7tByj;SSL Mode=Require;";

    public static async Task<bool> CreateUserAsync(string username, string password)
    {
        try
        {
            using var conn = new NpgsqlConnection(DbConnectionString);
            await conn.OpenAsync();

            string sql = "INSERT INTO users (username, password_hash) VALUES (@u, @p)";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("u", username);
            cmd.Parameters.AddWithValue("p", passwordHash);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            return false;
        }
    }

    public static async Task<int?> ValidateUserCredentialsAsync(string username, string password)
    {
        using var conn = new NpgsqlConnection(DbConnectionString);
        await conn.OpenAsync();

        // שולפים גם את ה-id וגם את ה-password_hash
        string sql = "SELECT id, password_hash FROM users WHERE username = @u";

        using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("u", username);

        using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            int userId = reader.GetInt32(0);
            string savedHash = reader.GetString(1);

            if (BCrypt.Net.BCrypt.Verify(password, savedHash))
            {
                return userId; // מחזירים את ה-ID הבינארי של המשתמש
            }
        }

        return null;
    }
}