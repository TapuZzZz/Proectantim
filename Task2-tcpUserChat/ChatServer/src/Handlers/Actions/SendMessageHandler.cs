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
        if (!messageData.IsGroup)
        {
            // שליפת ה-Stream של הנמען מהמילון המרכזי
            var recipientStream = ClientManager.GetClientStream(messageData.TargetId);

            if (recipientStream != null)
            {
                // הכנת ה-Notification שיישלח ישירות לנמען
                var incomingMessage = new
                {
                    FromUserId = session.UserId.Value,
                    FromUsername = session.Username,
                    Content = messageData.Content,
                    SentAt = DateTime.UtcNow
                };

                string notificationJson = JsonConverterHelper.Serialize(incomingMessage);
                byte[] notificationBytes = Encoding.UTF8.GetBytes(notificationJson);

                // דחיפת ההודעה בזמן אמת לנמען
                await recipientStream.WriteAsync(notificationBytes, 0, notificationBytes.Length);
                Console.WriteLine($"[Real-Time] Message delivered live to User ID {messageData.TargetId}");
            }
            else
            {
                Console.WriteLine($"[Real-Time] User ID {messageData.TargetId} is offline. Message saved in DB only.");
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