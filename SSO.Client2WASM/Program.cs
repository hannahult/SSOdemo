using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace SSO.Client2WASM
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            


            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });



            builder.Services.AddOidcAuthentication(options =>
            {
                builder.Configuration.Bind("Local", options.ProviderOptions);

                options.ProviderOptions.DefaultScopes.Clear();

                options.ProviderOptions.Authority = "https://localhost:7013";
                options.ProviderOptions.ClientId = "client2";
                options.ProviderOptions.ResponseType = "code";
                options.ProviderOptions.ResponseMode = "query";
                options.ProviderOptions.DefaultScopes.Add("openid");
                options.ProviderOptions.DefaultScopes.Add("profile");
                options.ProviderOptions.DefaultScopes.Add("email");

                options.AuthenticationPaths.LogInPath = "authentication/login";
                options.AuthenticationPaths.LogOutPath = "authentication/logout";
                options.AuthenticationPaths.LogInCallbackPath = "authentication/login-callback";
                options.AuthenticationPaths.LogOutCallbackPath = "authentication/logout-callback";
                options.AuthenticationPaths.ProfilePath = "authentication/profile";

                options.ProviderOptions.RedirectUri = "https://localhost:7193/authentication/login-callback";
                options.ProviderOptions.PostLogoutRedirectUri = "https://localhost:7193/";
                options.ProviderOptions.MetadataUrl = "https://localhost:7013/.well-known/openid-configuration";



            });

            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            await builder.Build().RunAsync();
        }
    }
}
