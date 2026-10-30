using System;
using System.Text;
using System.Text.Json;
using System.Net.Sockets;
using System.Threading.Tasks;
using ChatServer.Protocol;
using ChatServer.Payloads;
using ChatServer.Helpers;
using ChatServer.Services;

namespace ChatServer.Handlers.Actions;

public static class SendMessageHandler
{
    public static async Task HandleAsync(JsonElement payload, NetworkStream stream, UserSession session)
    {
        var response = new NetworkResponse<string>();

        // 1. אבטחה: בדיקה שהמשתמש מחובר
        if (!session.IsAuthenticated || !session.UserId.HasValue)
        {
            response.Success = false;
            response.Message = "Unauthorized: You must be logged in to send messages.";
            await SendResponseAsync(stream, response);
            return;
        }

        var messageData = JsonConverterHelper.Deserialize<SendMessagePayload>(payload.GetRawText());

        if (messageData == null || string.IsNullOrWhiteSpace(messageData.Content) || messageData.TargetId <= 0)
        {
            response.Success = false;
            response.Message = "Invalid message payload. Content and TargetId are required.";
            await SendResponseAsync(stream, response);
            return;
        }

        // 2. שמירת ההודעה ב-Database
        bool isSaved = await MessageService.SaveMessageAsync(
            session.UserId.Value, 
            messageData.TargetId, 
            messageData.IsGroup, 
            messageData.Content
        );

        if (!isSaved)
        {
            response.Success = false;
            response.Message = "Failed to save message to database.";
            await SendResponseAsync(stream, response);
            return;
        }

        // 3. הפצה בזמן אמת (Real-Time Broadcast)
        if (messageData.IsGroup)
        {
            // שליפת כל חברי הקבוצה מה-DB
            var memberIds = await GroupService.GetGroupMemberIdsAsync(messageData.TargetId);

            var groupMessage = new
            {
                GroupId = messageData.TargetId,
                FromUserId = session.UserId.Value,
                FromUsername = session.Username,
                Content = messageData.Content,
                IsGroup = true,
                SentAt = DateTime.UtcNow
            };

            string notificationJson = JsonConverterHelper.Serialize(groupMessage);
            byte[] notificationBytes = Encoding.UTF8.GetBytes(notificationJson);

            foreach (int memberId in memberIds)
            {
                // לא שולחים שוב לשולח עצמו
                if (memberId == session.UserId.Value) continue;

                var memberStream = ClientManager.GetClientStream(memberId);
                if (memberStream != null)
                {
                    await memberStream.WriteAsync(notificationBytes, 0, notificationBytes.Length);
                    Console.WriteLine($"[Real-Time Group] Message delivered live to User ID {memberId} in Group {messageData.TargetId}");
                }
            }
        }
        else
        {
            // הודעה פרטית (כפי שהיה מוגדר קודם)
            var recipientStream = ClientManager.GetClientStream(messageData.TargetId);
            if (recipientStream != null)
            {
                var incomingMessage = new
                {
                    FromUserId = session.UserId.Value,
                    FromUsername = session.Username,
                    Content = messageData.Content,
                    IsGroup = false,
                    SentAt = DateTime.UtcNow
                };

                string notificationJson = JsonConverterHelper.Serialize(incomingMessage);
                byte[] notificationBytes = Encoding.UTF8.GetBytes(notificationJson);

                await recipientStream.WriteAsync(notificationBytes, 0, notificationBytes.Length);
                Console.WriteLine($"[Real-Time] Message delivered live to User ID {messageData.TargetId}");
            }
        }

        // 4. החזרת תשובת אישור לשולח
        response.Success = true;
        response.Message = "Message sent successfully.";
        response.Data = messageData.Content;

        await SendResponseAsync(stream, response);
    }

    private static async Task SendResponseAsync(NetworkStream stream, NetworkResponse<string> response)
    {
        string jsonResponse = JsonConverterHelper.Serialize(response);
        byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
}