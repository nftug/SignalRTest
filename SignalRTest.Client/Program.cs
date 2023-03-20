using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Reactive.Bindings;
using SignalRTest.Server;

string? connectionId = null;
string? _userName = null;
string? previousPrompt = null;
List<User> userList = new();
ReactivePropertySlim<string?> accessToken = new();

var connection =
    new HubConnectionBuilder()
        .WithUrl("http://localhost:5090/hubs/chat", options =>
        {
            options.AccessTokenProvider = () => Task.FromResult(accessToken.Value);
        })
        .Build();

// イベントの設定

connection.On("Success", () => Console.WriteLine("操作が成功しました。"));
connection.On<Error>("Error", err => Console.WriteLine(err switch
{
    Error.UserNotFound => "ユーザーが見つかりません。",
    _ => "エラーが発生しました。"
}));

connection.On<User>("AddUser", user =>
{
    if (_userName is null) return;
    Console.WriteLine($"ユーザー {user.UserName} が入室しました。");
    if (!userList.Any(x => x == user)) userList.Add(user);
    if (previousPrompt != null) Console.WriteLine(previousPrompt);
});

connection.On<User>("RemoveUser", user =>
{
    if (_userName is null) return;
    Console.WriteLine($"ユーザー {user.UserName} が退室しました。");
    userList.Remove(user);
    if (previousPrompt != null) Console.WriteLine(previousPrompt);
});

connection.On<List<User>>("ReceiveUserList", v => userList = v);

connection.On<string>("ReceiveConnectionId", v => connectionId = v);

connection.On<Message>("ReceiveMessage", message =>
{
    Console.WriteLine($"{message.SentFrom}からメッセージを受け取りました。");
    Console.WriteLine(message.Content);
    if (previousPrompt != null) Console.WriteLine(previousPrompt);
});

while (_userName is null)
{
    Console.WriteLine("ようこそ。ユーザー名を入力してください。");

    if (Console.ReadLine() is { Length: > 0 } userName)
    {
        // REST APIでユーザーを登録してログイン
        var httpClient = new HttpClient();
        var response = await httpClient.PostAsJsonAsync("http://localhost:5090/login", new User(Guid.NewGuid().ToString(), userName));
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result?.Token != null)
        {
            Console.WriteLine("ログインしました。");
            _userName = userName;
            accessToken.Value = result.Token;
        }
        else
        {
            Console.WriteLine("そのユーザー名は既に使われています。");
        }
    }
}

// 接続開始
_ = Task.Run(() => connection.StartAsync());

if (!userList.Any(x => x.UserName != _userName))
{
    Console.WriteLine("他のメンバーの入室を待っています…");
    while (!userList.Any(x => x.UserName != _userName))
    {
        await connection.InvokeAsync("GetUserList");
        await Task.Delay(100);
    }
}

while (true)
{
    var sentTo = GetSentTo("送信先を指定してください。(数値以外で終了)");
    if (sentTo is null) break;
    var message = ReadLine("メッセージを指定してください。(空白文字で終了)");
    if (message is null) break;

    previousPrompt = null;
    await connection.InvokeAsync("SendMessage", sentTo.UserName, message);
    await Task.Delay(100);
}

// 接続終了
await connection.StopAsync();

User? GetSentTo(string prompt)
{
    previousPrompt = prompt;
    while (true)
    {
        Console.WriteLine(prompt);
        userList
            .Select((user, i) => (user, i)).ToList()
            .ForEach(x => Console.WriteLine($"{x.i}: {x.user.UserName}"));
        string? input = Console.ReadLine();

        if (!int.TryParse(input, out var index)) return null;
        if (index >= userList.Count || index < 0) continue;
        return userList[index];
    }
}

string? ReadLine(string message)
{
    while (true)
    {
        previousPrompt = message;
        Console.WriteLine(message);
        if (Console.ReadLine() is not { Length: > 0 } sentTo) return null;
        return sentTo;
    }
}