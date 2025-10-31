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

                options.ProviderOptions.Authority = "https://ssoidenity-bdd9gagyezgwgjbh.swedencentral-01.azurewebsites.net";
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

                options.ProviderOptions.RedirectUri = "https://ssoclient2-bpash8cshtbggqh8.swedencentral-01.azurewebsites.net/authentication/login-callback";
                options.ProviderOptions.PostLogoutRedirectUri = "https://ssoclient2-bpash8cshtbggqh8.swedencentral-01.azurewebsites.net/";
                options.ProviderOptions.MetadataUrl = "https://ssoclient2-bpash8cshtbggqh8.swedencentral-01.azurewebsites.net/.well-known/openid-configuration";



            });

            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            await builder.Build().RunAsync();
        }
    }
}
