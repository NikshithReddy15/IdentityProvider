using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationAPI.Controllers;

[Authorize]
public class BaseAuthorizationController : ControllerBase
{
}