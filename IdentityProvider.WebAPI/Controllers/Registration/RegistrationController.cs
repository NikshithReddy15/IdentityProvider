using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace AuthenticationAPI.Controllers.Registration;

[ApiController]
[Route("api/registration")]
public class RegistrationController (IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost]
    public async Task CreateUserAsync(RegisterUserDto registerUserDto)
    {
        await authenticationService.CreateUserAsync(registerUserDto);
    }
}