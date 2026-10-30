using System.Text.Json;

namespace ChatServer.Helpers;

public static class JsonConverterHelper
{
    // המרת אובייקט לגייסון
    public static string Serialize<T>(T data)
    {
        return JsonSerializer.Serialize(data) + "\n";
    }

    // תרגום גייסון לאובייט
    public static T? Deserialize<T>(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(jsonString);
    }
}