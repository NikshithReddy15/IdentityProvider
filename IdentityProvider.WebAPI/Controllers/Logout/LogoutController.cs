using System.IdentityModel.Tokens.Jwt;
using Extensions;
using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace IdentityProvider.WebAPI.Controllers.Logout;

[ApiController]
[Route("api/logout")]
public class LogoutController(IAuthenticationService authenticationService, JwtSecurityTokenHandler jwtSecurityTokenHandler) : BaseAuthorizationController
{
    [HttpPost]
    public async Task LogoutAsync()
    {
        var bearer = Request.Headers.Authorization.ToString();
        var jwtToken = bearer.Replace("Bearer ", string.Empty).Trim();
        var expiresOn = jwtSecurityTokenHandler.ReadJwtToken(jwtToken).ValidTo;
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.ToGuid() ?? Guid.Empty;
        var sessionId = User.FindFirst(JwtRegisteredClaimNames.Sid)?.Value.ToGuid() ?? Guid.Empty;
        var sessionDetailsDto = new SessionDetailsDto { UserId = userId, SessionId = sessionId };
        await authenticationService.LogoutAsync(sessionDetailsDto, jwtToken, expiresOn);
    }
}
