using Microsoft.AspNetCore.Mvc;

namespace IdentityProvider.WebAPI.Controllers.Authentication;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController() : BaseAuthorizationController
{
    [HttpGet]
    public IResult Authenticate()
    {
        return Results.Ok(200);
    }
}