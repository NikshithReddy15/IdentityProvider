using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;
using Services.Authentication;

namespace IdentityProvider.WebAPI.Controllers.TokenController;

[ApiController]
[Route("api/token")]
public class TokenController (IAuthenticationService authenticationService, JwtSecurityTokenHandler jwtSecurityTokenHandler) : BaseAuthorizationController
{
    [HttpPost("refresh")]
    public async Task<TokenDetailsDto> RefreshTokenAsync(SessionDetailsDto sessionDetailsDto)
    {
        var bearer = Request.Headers.Authorization.ToString();
        var jwtToken = bearer.Replace("Bearer ", string.Empty).Trim();
        var expiresOn = jwtSecurityTokenHandler.ReadJwtToken(jwtToken).ValidTo;
        return await authenticationService.RefreshTokenAsync(sessionDetailsDto, jwtToken, expiresOn);
    }
}