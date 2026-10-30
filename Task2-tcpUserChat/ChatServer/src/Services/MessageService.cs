using System;
using System.Threading.Tasks;
using Npgsql;

namespace ChatServer.Services;

public static class MessageService
{
    private const string DbConnectionString = "Host=ep-lively-darkness-bavzh4f1.c-8.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_Gs8hbWM7tByj;SSL Mode=Require;";

    public static async Task<bool> SaveMessageAsync(int senderId, int targetId, bool isGroup, string content)
    {
        try
        {
            using var conn = new NpgsqlConnection(DbConnectionString);
            await conn.OpenAsync();

            string sql = @"INSERT INTO messages (sender_id, target_id, is_group, content) 
                          VALUES (@s, @t, @g, @c)";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("s", senderId);
            cmd.Parameters.AddWithValue("t", targetId);
            cmd.Parameters.AddWithValue("g", isGroup);
            cmd.Parameters.AddWithValue("c", content);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Database error saving message: {ex.Message}");
            return false;
        }
    }
}