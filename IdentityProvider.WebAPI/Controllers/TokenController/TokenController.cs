using System.IdentityModel.Tokens.Jwt;
using Extensions;
using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace IdentityProvider.WebAPI.Controllers.TokenController;

[ApiController]
[Route("api/token")]
public class TokenController(IAuthenticationService authenticationService, JwtSecurityTokenHandler jwtSecurityTokenHandler) : BaseAuthorizationController
{
    [HttpPost("refresh")]
    public async Task<TokenDetailsDto> RefreshTokenAsync()
    {
        var bearer = Request.Headers.Authorization.ToString();
        var jwtToken = bearer.Replace("Bearer ", string.Empty).Trim();
        var expiresOn = jwtSecurityTokenHandler.ReadJwtToken(jwtToken).ValidTo;
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.ToGuid() ?? Guid.Empty;
        var sessionId = User.FindFirst(JwtRegisteredClaimNames.Sid)?.Value.ToGuid() ?? Guid.Empty;
        var sessionDetailsDto = new SessionDetailsDto { UserId = userId, SessionId = sessionId };
        return await authenticationService.RefreshTokenAsync(sessionDetailsDto, jwtToken, expiresOn);
    }
}
