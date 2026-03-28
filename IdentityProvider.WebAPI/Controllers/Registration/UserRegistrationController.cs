using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace IdentityProvider.WebAPI.Controllers.Registration;

[ApiController]
[Route("api/user/registration")]
public class UserRegistrationController (IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost]
    public async Task CreateUserAsync(RegisterUserDto registerUserDto)
    {
        await authenticationService.CreateUserAsync(registerUserDto);
    }

    [HttpPost("verify-otp")]
    public async Task VerifyOtpAsync(VerifyOtpDto verifyOtpDto)
    {
        await authenticationService.VerifyRegistrationOtpAsync(verifyOtpDto);
    }
}