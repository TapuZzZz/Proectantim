using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;

namespace ChatServer.Services;

public static class GroupService
{
    private const string DbConnectionString = "Host=ep-lively-darkness-bavzh4f1.c-8.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_Gs8hbWM7tByj;SSL Mode=Require;";

    // יצירת קבוצה והוספת המייסד כחבר קבוצה
    public static async Task<int?> CreateGroupAsync(string groupName, int creatorUserId)
    {
        try
        {
            using var conn = new NpgsqlConnection(DbConnectionString);
            await conn.OpenAsync();

            string sqlGroup = "INSERT INTO groups (name) VALUES (@n) RETURNING id";
            using var cmdGroup = new NpgsqlCommand(sqlGroup, conn);
            cmdGroup.Parameters.AddWithValue("n", groupName);

            int groupId = Convert.ToInt32(await cmdGroup.ExecuteScalarAsync());

            // הוספת יוצר הקבוצה כחבר
            await AddMemberToGroupAsync(groupId, creatorUserId);

            return groupId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error creating group: {ex.Message}");
            return null;
        }
    }

    // הוספת חבר לקבוצה
    public static async Task<bool> AddMemberToGroupAsync(int groupId, int userId)
    {
        try
        {
            using var conn = new NpgsqlConnection(DbConnectionString);
            await conn.OpenAsync();

            string sql = "INSERT INTO group_members (group_id, user_id) VALUES (@g, @u) ON CONFLICT DO NOTHING";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("g", groupId);
            cmd.Parameters.AddWithValue("u", userId);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error adding member: {ex.Message}");
            return false;
        }
    }

    // שליפת כל ה-UserIds של חברי הקבוצה (לצורך הפצת הודעות בלייב)
    public static async Task<List<int>> GetGroupMemberIdsAsync(int groupId)
    {
        var memberIds = new List<int>();
        try
        {
            using var conn = new NpgsqlConnection(DbConnectionString);
            await conn.OpenAsync();

            string sql = "SELECT user_id FROM group_members WHERE group_id = @g";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("g", groupId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                memberIds.Add(reader.GetInt32(0));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Error fetching group members: {ex.Message}");
        }

        return memberIds;
    }
}