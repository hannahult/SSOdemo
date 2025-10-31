using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;

namespace SSO.CLient1MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie()

            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "https://ssoidenity-bdd9gagyezgwgjbh.swedencentral-01.azurewebsites.net"; 
                options.ClientId = "client1";
                options.ClientSecret = "secret";
                options.ResponseType = "code";
                options.UsePkce = false;
                options.SaveTokens = true;              
                options.ResponseMode = "form_post";

                options.CallbackPath = "/signin-oidc";
                options.SignedOutCallbackPath = "/signout-callback-oidc";
                options.RequireHttpsMetadata = false;
                options.SignedOutRedirectUri = "https://ssoclient1-ccasghfwb3frh8er.swedencentral-01.azurewebsites.net";

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
