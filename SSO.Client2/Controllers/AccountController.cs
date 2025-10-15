using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SSO.Client2.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [HttpPost]
        [Authorize]
        public IActionResult Login(string? returnUrl = "/")
        {
            //Redirect to the identity provider for authentication
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "oidc");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            // Logout of both the application and the identity provider
            return SignOut("Cookies", "oidc");
        }
    }
}
