using System;
using System.Threading.Tasks;
using Npgsql;

namespace ChatServer.Services;
using ChatServer.Payloads;

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

    public static async Task<List<ChatMessageDto>> GetChatHistoryAsync(int currentUserId, int targetId, bool isGroup)
    {
        var messages = new List<ChatMessageDto>();

        try
        {
            using var conn = new NpgsqlConnection(DbConnectionString);
            await conn.OpenAsync();

            string sql;

            if (isGroup)
            {
                // שליפת הודעות קבוצה
                sql = @"SELECT id, sender_id, target_id, is_group, content, created_at 
                        FROM messages 
                        WHERE target_id = @t AND is_group = TRUE 
                        ORDER BY created_at ASC LIMIT 50";
            }
            else
            {
                // שליפת שיחה פרטית (הודעות שנשלחו מ-A ל-B או מ-B ל-A)
                sql = @"SELECT id, sender_id, target_id, is_group, content, created_at 
                        FROM messages 
                        WHERE is_group = FALSE AND 
                              ((sender_id = @u AND target_id = @t) OR (sender_id = @t AND target_id = @u)) 
                        ORDER BY created_at ASC LIMIT 50";
            }

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("t", targetId);
            if (!isGroup)
            {
                cmd.Parameters.AddWithValue("u", currentUserId);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new ChatMessageDto
                {
                    Id = reader.GetInt32(0),
                    SenderId = reader.GetInt32(1),
                    TargetId = reader.GetInt32(2),
                    IsGroup = reader.GetBoolean(3),
                    Content = reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error fetching chat history: {ex.Message}");
        }

        return messages;
    }
}