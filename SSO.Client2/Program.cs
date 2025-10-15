using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace SSO.Client2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.AddFilter("Microsoft.AspNetCore.Authentication", LogLevel.Debug);
            builder.Logging.AddFilter("Microsoft.IdentityModel.Protocols.OpenIdConnect", LogLevel.Debug);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie()

            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "https://localhost:7013";
                options.ClientId = "client2";
                //options.ClientSecret = "secret";
                options.ResponseType = "code";
                options.UsePkce = true;
                options.SaveTokens = true;
               
                options.ResponseMode = "form_post";

                options.CallbackPath = "/signin-oidc";
                options.SignedOutCallbackPath = "/signout-callback-oidc";
                options.RequireHttpsMetadata = true;
                options.SignedOutRedirectUri = "https://localhost:7103";

                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

            });

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapDefaultControllerRoute();

            app.Run();
        }
    }
}
