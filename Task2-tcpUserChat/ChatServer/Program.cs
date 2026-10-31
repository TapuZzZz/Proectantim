namespace ChatServer;

public class Program
{
    private const int ServerPort = 8080;
    // משאיר גלוי כי אין לי חשיבות אם זה יודלף בגלל הגיטהאב
    private const string DbConnectionString = "Host=ep-lively-darkness-bavzh4f1.c-8.eu-central-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_Gs8hbWM7tByj;SSL Mode=Require;";

    public static async Task Main()
    {
        var database = new Database(DbConnectionString);
        await database.TestConnectionAsync();
        Console.WriteLine("Connected to database");

        var server = new Server(ServerPort, database);
        await server.RunAsync();
    }
}