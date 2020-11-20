using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KeycloakUserProfileUi.Controllers
{
  public class SecurityController : BaseController
  {
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(ILogger<SecurityController> logger)
    {
      _logger = logger;
    }

    public IActionResult SignIn()
    {
      if (!HttpContext.User.Identity.IsAuthenticated)
      {
        return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
      }

      return RedirectToAction("Index", "Home");
    }

    public IActionResult SignOut() => new SignOutResult(new[]
      {
        OpenIdConnectDefaults.AuthenticationScheme,
        CookieAuthenticationDefaults.AuthenticationScheme,
      },
      new AuthenticationProperties
      {
        RedirectUri = "/"
      });
  }
}
