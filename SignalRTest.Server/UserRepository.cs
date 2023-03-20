using Microsoft.AspNetCore.SignalR;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace SignalRTest.Server;

public class UserRepository
{
    private readonly IHubContext<ChatHub> _chatHub;

    public UserRepository(IHubContext<ChatHub> chatHub)
    {
        _chatHub = chatHub;

        UserList
            .ObserveAddChanged()
            .Subscribe(async v => await _chatHub.Clients.All.SendAsync("AddUser", v));
        UserList
            .ObserveRemoveChanged()
            .Subscribe(async v => await _chatHub.Clients.All.SendAsync("RemoveUser", v));
    }

    public ReactiveCollection<User> UserList { get; } = new();

    public bool Add(User user)
    {
        if (UserList.Any(x => x.UserName == user.UserName)) return false;
        UserList.AddOnScheduler(user);
        return true;
    }

    public void Remove(string userId)
    {
        if (!UserList.Any(x => x.UserId == userId)) return;
        UserList.RemoveOnScheduler(UserList.First(x => x.UserId == userId));
    }
}