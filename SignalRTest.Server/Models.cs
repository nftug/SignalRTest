namespace SignalRTest.Server;

public record User(string ConnectionId, string UserName);

public record Message(string Content, string SentFrom);

public enum Error
{
    UserNotFound
}

public record LoginResponse(bool IsSucceed);