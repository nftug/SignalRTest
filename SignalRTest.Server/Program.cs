using Microsoft.AspNetCore.Mvc;
using SignalRTest.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<UserRepository>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapHub<ChatHub>("/chat/hub");
app.MapPost(
    "/login",
    ([FromServices] UserRepository repository, User request)
        => repository.Add(request) ? new LoginResponse(true) : new(false)
);

app.Run();
