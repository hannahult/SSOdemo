using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SSO.IdentityServer.Controllers
{
    public class AuthorizationController : Controller
    {
        [HttpGet("/connect/authorize")]
        public IActionResult Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            return View("Authorize", request);

        }


        [HttpPost("/connect/authorize")]
        [ValidateAntiForgeryToken]
        public IActionResult Accept(string submit)
        {
            var request = HttpContext.GetOpenIddictServerRequest();

            if (submit == "accept")
            {
                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                identity.AddClaim(new Claim(Claims.Subject, "1").SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
                identity.AddClaim(new Claim(Claims.Name, "admin").SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

                var principal = new ClaimsPrincipal(identity);
                principal.SetScopes(request.GetScopes());
                principal.SetResources("resource_server");

                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                return BadRequest(new
                {
                    error = "invalid_request",
                    error_description = "The OpenIddict request could not be retrieved."
                });
            }

            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result?.Principal == null)
            {
                return BadRequest(new
                {
                    error = "invalid_grant",
                    error_description = "The authentication information could not be retrieved."
                });
            }

            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
    }
}
