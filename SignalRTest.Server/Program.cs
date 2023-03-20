using SignalRTest.Server;
using SignalRTest.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/hubs/chat");
app.MapControllers();

app.Run();
