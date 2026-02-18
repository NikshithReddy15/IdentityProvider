using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace IdentityProvider.WebAPI.Controllers.Logout;

[ApiController]
[Route("api/logout")]
public class LogoutController (IAuthenticationService authenticationService, JwtSecurityTokenHandler jwtSecurityTokenHandler) : BaseAuthorizationController
{
    [HttpPost]
    public async Task LogoutAsync(SessionDetailsDto sessionDetailsDto)
    {
        var bearer = Request.Headers.Authorization.ToString();
        var jwtToken = bearer.Replace("Bearer ", string.Empty).Trim();
        var expiresOn = jwtSecurityTokenHandler.ReadJwtToken(jwtToken).ValidTo;
        await authenticationService.LogoutAsync(sessionDetailsDto, jwtToken, expiresOn);
    }
}