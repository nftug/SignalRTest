using Microsoft.AspNetCore.SignalR;

namespace SignalRTest.Server;

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
        var currentUser = _userRepository.UserList.First(x => x.ConnectionId == Context.ConnectionId);
        _userRepository.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public Task GetUserList()
        => Clients.Caller.SendAsync("ReceiveUserList", _userRepository.UserList.ToList());

    public async Task SendMessage(string userName, string message)
    {
        var connectionId = _userRepository.FindConnectionId(userName);
        if (connectionId is null)
        {
            await Clients.Caller.SendAsync("Error", Error.UserNotFound);
            return;
        }

        var sentFrom = _userRepository.GetUserName(Context.ConnectionId);
        await Clients.Client(connectionId).SendAsync("ReceiveMessage", new Message(message, sentFrom));
        await Clients.Caller.SendAsync("Success");
    }
}