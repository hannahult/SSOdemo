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
                options.ClientSecret = "secret";
                options.ResponseType = "code";
                options.SaveTokens = true;
                options.UsePkce = false;
                options.CallbackPath = "/signin-oidc";
                options.RequireHttpsMetadata = false;

                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                options.Events.OnTokenResponseReceived = context =>
                {
                    Console.WriteLine("Token received:");
                    Console.WriteLine(context.TokenEndpointResponse.AccessToken);
                    Console.WriteLine(context.TokenEndpointResponse.IdToken);
                    return Task.CompletedTask;
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
