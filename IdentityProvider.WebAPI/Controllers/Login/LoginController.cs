using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace IdentityProvider.WebAPI.Controllers.Login;

[ApiController]
[Route("api/login")]
public class LoginController (IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost]
    public async Task<string> LoginAsync(LoginDetailsDto loginDetailsDto)
    {
        return await authenticationService.LoginAsync(loginDetailsDto);
    }
}