using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace IdentityProvider.WebAPI.Controllers.Login;

[ApiController]
[Route("api/login")]
public class LoginController (IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost]
    public async Task<TokenDetailsDto> LoginAsync(LoginDetailsDto loginDetailsDto)
    {
        var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.HttpContext.Request.Headers["User-Agent"].ToString();
        return await authenticationService.LoginAsync(loginDetailsDto, userAgent, ipAddress);
    }
}