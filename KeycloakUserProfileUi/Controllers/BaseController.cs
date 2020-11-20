using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KeycloakUserProfileUi.Models;

namespace KeycloakUserProfileUi.Controllers
{
  public class BaseController : Controller
  {
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
  }
}
