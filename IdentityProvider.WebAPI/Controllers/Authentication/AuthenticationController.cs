using Microsoft.AspNetCore.Mvc;
using Models.DataTransferObjects;

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