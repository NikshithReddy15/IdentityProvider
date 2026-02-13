using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityProvider.WebAPI.Controllers;

[Authorize]
public class BaseAuthorizationController : ControllerBase
{
}