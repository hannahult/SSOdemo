using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;


namespace SSO.Client2Blazor
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
                options.ProviderOptions.Authority = "https://localhost:7013"; 
                options.ProviderOptions.ClientId = "client2";
                options.ProviderOptions.ResponseType = "code"; 
                options.ProviderOptions.DefaultScopes.Add("openid");
                options.ProviderOptions.DefaultScopes.Add("profile");
                options.ProviderOptions.DefaultScopes.Add("email");

                options.AuthenticationPaths.LogInCallbackPath = "authentication/login-callback";
                options.AuthenticationPaths.LogOutCallbackPath = "authentication/logout-callback";
                options.AuthenticationPaths.RemoteProfilePath = "authentication/profile";
                options.AuthenticationPaths.LogOutPath = "authentication/logout";
            });

            await builder.Build().RunAsync();
        }
    }
}
