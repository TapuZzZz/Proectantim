namespace ChatServer.Protocol;

public class NetworkRequest<T>
{
    public ActionType Action { get; set; }
    public T? Payload { get; set; }
}