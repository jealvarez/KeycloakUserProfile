using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace KeycloakUserProfileUi.Controllers
{
  public class AccountController : BaseController
  {
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
    {
      _logger = logger;
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    [Authorize]
    public IActionResult Index()
    {
      ViewData["accountUrl"] = Configuration["Keycloak:AuthorityUrl"] + "/account?referrer=" + Configuration["Keycloak:ClientId"] + "&referrer_uri=" + UriHelper.GetEncodedUrl(HttpContext.Request);

      return View();
    }
  }
}
