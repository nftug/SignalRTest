namespace SignalRTest.Server;

public record User(string UserId, string UserName);

public record Message(string Content, string SentFrom);

public enum Error
{
    UserNotFound
}

public record JwtSettings(string Secret, string SiteUrl, string JwtExpireDay)
{
    public JwtSettings() : this(string.Empty, string.Empty, string.Empty) { }
}

public record LoginResult(string? Token);