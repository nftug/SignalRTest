using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignalRTest.Server.Services;

namespace SignalRTest.Server.Controllers;

[ApiController, Route("/")]
public class UsersController : ControllerBase
{
    protected AuthTokenService AuthTokenService
        => HttpContext.RequestServices.GetRequiredService<AuthTokenService>();

    protected UserRepository Repository
        => HttpContext.RequestServices.GetRequiredService<UserRepository>();

    [AllowAnonymous, HttpPost("login")]
    public IActionResult Login(User command)
    {
        if (!Repository.Add(command)) return Ok(new LoginResult(null));
        var token = AuthTokenService.CreateToken(command.UserName, command.UserId);
        return Ok(new LoginResult(token));
    }
}
