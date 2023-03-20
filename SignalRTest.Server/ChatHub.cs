using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRTest.Server;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ChatHub : Hub
{
    private readonly UserRepository _userRepository;

    public ChatHub(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("ReceiveConnectionId", Context.ConnectionId);
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _userRepository.Remove(Context.UserIdentifier!);
        return base.OnDisconnectedAsync(exception);
    }

    public Task GetUserList()
        => Clients.Caller.SendAsync("ReceiveUserList", _userRepository.UserList.ToList());

    public async Task SendMessage(string userName, string message)
    {
        var sentFrom = _userRepository.UserList.First(x => x.UserId == Context.UserIdentifier);
        var sentTo = _userRepository.UserList.FirstOrDefault(x => x.UserName == userName);
        if (sentTo is null)
        {
            await Clients.Caller.SendAsync("Error", Error.UserNotFound);
            return;
        }

        Console.WriteLine($"Sent from: {sentFrom.UserName}");
        Console.WriteLine($"Sent to: {sentTo.UserName}");
        await Clients.User(sentTo.UserId).SendAsync("ReceiveMessage", new Message(message, sentFrom.UserName));

        await Clients.Caller.SendAsync("Success");
    }
}